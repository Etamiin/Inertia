using Inertia.Network;
using Inertia.Paper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Inertia.IO;
using Inertia.Logging;
using System.Diagnostics;

namespace Inertia
{
    internal static class ReflectionProvider
    {
        internal static bool IsPaperOwned { get; private set; }
        internal static bool ContainsNetworkServerEntities { get; private set; }
        
        private readonly static Dictionary<Type, Dictionary<string, SerializablePropertyCache>> _propertyCaches;
        private readonly static Dictionary<Type, SerializableObjectCache> _serializableObjCaches;
        private readonly static Dictionary<string, Type> _commands;
        private readonly static Dictionary<ushort, Type> _messageTypes;
        private readonly static Dictionary<Type, NetworkMessageHandler> _messagesHandlerPerEntity;
        private readonly static Dictionary<Type, Type> _indirectNetworkEntityTypes;

        static ReflectionProvider()
        {
            _propertyCaches = new Dictionary<Type, Dictionary<string, SerializablePropertyCache>>();
            _serializableObjCaches = new Dictionary<Type, SerializableObjectCache>();
            _commands = new Dictionary<string, Type>();
            _messageTypes = new Dictionary<ushort, Type>();
            _messagesHandlerPerEntity = new Dictionary<Type, NetworkMessageHandler>();
            _indirectNetworkEntityTypes = new Dictionary<Type, Type>();

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
        internal static IEnumerable<string> GetAllCommandNames()
        {
            return _commands.Keys.AsEnumerable();
        }
        internal static CommandLine CreateCommand(string commandName, ILogger logger, object? state)
        {
            if (_commands.TryGetValue(commandName, out var cmdType))
            {
                var cmd = TryCreateInstance<CommandLine>(BindingFlags.Public | BindingFlags.Instance, cmdType, Type.EmptyTypes);
                
                cmd.Logger = logger;
                cmd.State = state;

                return cmd;
            }

            return null;
        }
        internal static bool TryCreateNetworkMessage(ushort messageId, out NetworkMessage message)
        {
            if (_messageTypes.TryGetValue(messageId, out var messageType))
            {
                message = (NetworkMessage)GetSerializableObjectCache(messageType)?.CreateInstance();
            }
            else
            {
                message = null;
            }

            return message != null;
        }
        internal static bool TryGetMessageHandler(NetworkEntity receiver, out NetworkMessageHandler handler)
        {
            return 
                _messagesHandlerPerEntity.TryGetValue(receiver.GetType(), out handler) ||
                (_indirectNetworkEntityTypes.TryGetValue(receiver.GetType(), out var indirectType) && _messagesHandlerPerEntity.TryGetValue(indirectType, out handler));
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
                if (_commands.ContainsKey(instance.Name))
                {
                    throw new DuplicateNameException($"Command '{instance.Name}' already registered.");
                }

                _commands[instance.Name] = type;
            }

            if (!IsPaperOwned && type.GetCustomAttribute<PaperOwnerAttribute>() != null)
            {
                IsPaperOwned = true;
            }
        }
        private static void ReadTypeNetworkInformations(Type type)
        {
            if (!ContainsNetworkServerEntities)
            {
                ContainsNetworkServerEntities = type.IsSubclassOf(typeof(TcpServerEntity)) || type.IsSubclassOf(typeof(WebSocketServerEntity));
            }
   
            if (type.IsSubclassOf(typeof(NetworkMessage)))
            {
                if (type.IsAbstract) return;

                var message = (NetworkMessage)GetSerializableObjectCache(type)?.CreateInstance();
                if (!_messageTypes.ContainsKey(message.MessageId))
                {
                    _messageTypes.Add(message.MessageId, type);
                }
            }
            else if (type.IsSubclassOf(typeof(NetworkEntity)))
            {
                var indirectEntityType = type
                    .GetInterfaces()
                    .FirstOrDefault((interfaceType) => interfaceType.GetCustomAttribute<IndirectNetworkEntityAttribute>() != null);

                if (indirectEntityType != null)
                {
                    _indirectNetworkEntityTypes.Add(type, indirectEntityType);
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
                        var isValidEntity = 
                            ps[1].ParameterType.IsSubclassOf(typeof(NetworkClientEntity)) ||
                            ps[1].ParameterType.IsSubclassOf(typeof(NetworkConnectionEntity)) ||
                            ps[1].ParameterType.GetCustomAttribute<IndirectNetworkEntityAttribute>() != null;

                        if (!isValidEntity) continue;

                        var msgType = ps[0].ParameterType;
                        var entityType = ps[1].ParameterType;

                        if (!_messagesHandlerPerEntity.TryGetValue(entityType, out var handler))
                        {
                            handler = new NetworkMessageHandler();
                            _messagesHandlerPerEntity.Add(entityType, handler);
                        }

                        handler.RegisterReference(msgType, smethod);
                    }
                }
            }
        }
    }
}