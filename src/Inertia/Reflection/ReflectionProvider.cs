using Inertia.Network;
using Inertia.Paper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Inertia.IO;

namespace Inertia
{
    internal static class ReflectionProvider
    {
        internal static bool IsPaperOwned { get; private set; }
        internal static bool IsNetworkClientUsed { get; private set; }
        internal static bool IsNetworkServerUsed { get; private set; }
        
        private readonly static Dictionary<Type, Dictionary<string, SerializablePropertyCache>> _propertyCaches;
        private readonly static Dictionary<Type, SerializableObjectCache> _serializableObjCaches;
        private readonly static Dictionary<string, CommandLine> _commands;
        private readonly static Dictionary<ushort, Type> _messageTypes;
        private readonly static Dictionary<Type, NetworkMessageHandler> _messagesHandlerPerEntity;

        static ReflectionProvider()
        {
            _propertyCaches = new Dictionary<Type, Dictionary<string, SerializablePropertyCache>>();
            _serializableObjCaches = new Dictionary<Type, SerializableObjectCache>();
            _commands = new Dictionary<string, CommandLine>();
            _messageTypes = new Dictionary<ushort, Type>();
            _messagesHandlerPerEntity = new Dictionary<Type, NetworkMessageHandler>();

            RegisterAll();
        }

        internal static bool Invalidate() => true;

        internal static bool TryGetSerializableProperties(Type type, out Dictionary<string, SerializablePropertyCache> properties)
        {
            return _propertyCaches.TryGetValue(type, out properties);
        }
        internal static SerializableObjectCache? GetSerializableObjectCache(Type type)
        {
            if (_serializableObjCaches.TryGetValue(type, out var cache)) return cache;

            return default;
        }
        internal static IEnumerable<CommandLine> GetAllCommands()
        {
            return _commands.Values.AsEnumerable();
        }
        internal static bool TryGetCommand(string commandName, out CommandLine command)
        {
            return _commands.TryGetValue(commandName, out command);
        }
        internal static bool TryGetMessageType(ushort messageId, out Type messageType)
        {
            return _messageTypes.TryGetValue(messageId, out messageType);
        }
        internal static bool TryGetMessageHandler(NetworkEntity receiver, out NetworkMessageHandler handler)
        {
            if (_messagesHandlerPerEntity.TryGetValue(receiver.GetType(), out handler) ||
                _messagesHandlerPerEntity.TryGetValue(receiver.IndirectType, out handler))
            {
                return true;
            }

            return false;
        }

        private static void RegisterAll()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            try
            {
                var penSystemTypes = new List<Type>();
                foreach (var assembly in assemblies)
                {
                    var types = assembly.GetTypes()
                        .Where((t) => t.IsClass && !t.IsAbstract);

                    foreach (var type in types)
                    {
                        ReadTypeIOInformations(type);
                        ReadTypeInformations(type);
                        ReadTypeNetworkInformations(type);

                        if (typeof(IPenSystem).IsAssignableFrom(type))
                        {
                            penSystemTypes.Add(type);
                        }
                    }
                }

                //Creation of the Pen systems after reading all the types in all assemblies in case the Paper system is owned by a Type in a specific assembly
                //So we only initialize the pen systems once when every parameters are set.
                foreach (var penSystemType in penSystemTypes)
                {
                    TryCreateInstance<IPenSystem>(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, penSystemType, Type.EmptyTypes);
                }
            }
            catch (Exception ex)
            {
                throw new TypeLoadException($"{nameof(ReflectionProvider)} failed to load.", ex);
            }
        }
        private static T TryCreateInstance<T>(Type owner, Type[] parametersTypes, params object[] parameters)
        {
            return TryCreateInstance<T>(BindingFlags.Instance | BindingFlags.Public, owner, parametersTypes, parameters);
        }
        private static T TryCreateInstance<T>(BindingFlags bindingFlags, Type owner, Type[] parametersTypes, params object[] parameters)
        {
            var constructor = owner.GetConstructor(bindingFlags, Type.DefaultBinder, parametersTypes, new ParameterModifier[0]);
            if (constructor == null) throw new ConstructorNotFoundException(owner, parametersTypes);

            return (T)constructor.Invoke(parameters);
        }
        private static void ReadTypeIOInformations(Type type)
        {
            if (typeof(ISerializable).IsAssignableFrom(type))
            {
                _serializableObjCaches[type] = new SerializableObjectCache(type);
            }

            if (type.GetCustomAttribute<AutoSerializableAttribute>() != null)
            {
                if (type.GetCustomAttribute<IgnoreAttribute>() != null) return;

                var propertiesDict = new Dictionary<string, SerializablePropertyCache>();
                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where((p) => p.CanRead && p.CanWrite && p.GetCustomAttribute<IgnoreAttribute>() == null);

                foreach (var property in properties)
                {
                    propertiesDict.Add(property.Name, new SerializablePropertyCache(property));
                }

                _propertyCaches.Add(type, propertiesDict);
            }
        }
        private static void ReadTypeInformations(Type type)
        {
            if (type.IsSubclassOf(typeof(CommandLine)))
            {
                var instance = TryCreateInstance<CommandLine>(type, Type.EmptyTypes);
                if (!_commands.ContainsKey(instance.Name))
                {
                    _commands.Add(instance.Name, instance);
                }
            }

            if (!IsPaperOwned && type.GetCustomAttribute<PaperOwnerAttribute>() != null)
            {
                IsPaperOwned = true;
            }
        }
        private static void ReadTypeNetworkInformations(Type type)
        {
            if (!IsNetworkClientUsed && type.IsSubclassOf(typeof(NetworkClientEntity)))
            {
                IsNetworkClientUsed = true;
            }

            if (type.IsSubclassOf(typeof(TcpServerEntity)) || type.IsSubclassOf(typeof(WebSocketServerEntity)))
            {
                if (!IsNetworkServerUsed)
                {
                    IsNetworkServerUsed = true;
                }
            }      

            if (type.IsSubclassOf(typeof(NetworkMessage)))
            {
                var message = NetworkProtocolManager.CreateMessage(type);
                if (!_messageTypes.ContainsKey(message.MessageId))
                {
                    _messageTypes.Add(message.MessageId, type);
                }
            }
            else if (typeof(IMessageHandler).IsAssignableFrom(type))
            {
                var sMethods = type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic);
                if (sMethods.Length == 0) return;

                foreach (var smethod in sMethods)
                {
                    var ps = smethod.GetParameters();
                    if (ps.Length == 2 && ps[0].ParameterType.IsSubclassOf(typeof(NetworkMessage)))
                    {
                        var isValidEntity = ps[1].ParameterType.IsSubclassOf(typeof(NetworkClientEntity)) || ps[1].ParameterType.IsSubclassOf(typeof(NetworkConnectionEntity));
                        if (!isValidEntity)
                        {
                            var attr = ps[1].ParameterType.GetCustomAttribute<IndirectNetworkEntityAttribute>();
                            if (attr == null) continue;
                        }

                        var msgType = ps[0].ParameterType;
                        var entityType = ps[1].ParameterType;

                        if (!_messagesHandlerPerEntity.ContainsKey(entityType))
                        {
                            _messagesHandlerPerEntity.Add(entityType, new NetworkMessageHandler());
                        }

                        _messagesHandlerPerEntity[entityType].RegisterReference(msgType, smethod);
                    }
                }
            }
        }
    }
}