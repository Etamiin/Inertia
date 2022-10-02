using Inertia.IO;
using Inertia.Network;
using Inertia.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Inertia
{
    internal static class ReflectionProvider
    {
        internal class SerializableFieldMemory
        {
            internal readonly FieldInfo Info;
            internal readonly MethodInfo? SerializationMethodInfo;
            internal readonly MethodInfo? DeserializationMethodInfo;

            internal SerializableFieldMemory(FieldInfo info)
            {
                Info = info;
                var serAttr = info.GetCustomAttribute<SerializeWith>();
                var deserAttr = info.GetCustomAttribute<DeserializeWith>();

                if (serAttr != null)
                {
                    var method = info.DeclaringType.GetMethod(serAttr.MethodName, BindingFlags.NonPublic | BindingFlags.Instance);
                    if (method != null)
                    {
                        var parameters = method.GetParameters();
                        if (parameters.Length == 1 && parameters[0].ParameterType == typeof(BasicWriter))
                        {
                            SerializationMethodInfo = method;
                        }
                    }
                }
                if (deserAttr != null)
                {
                    var method = info.DeclaringType.GetMethod(deserAttr.MethodName, BindingFlags.NonPublic | BindingFlags.Instance);
                    if (method != null)
                    {
                        var parameters = method.GetParameters();
                        if (parameters.Length == 1 && parameters[0].ParameterType == typeof(BasicReader))
                        {
                            DeserializationMethodInfo = method;
                        }
                    }
                }
            }

            public void Write(IAutoSerializable serializableObject, BasicWriter writer)
            {
                if (SerializationMethodInfo == null)
                {
                    var fieldValue = Info.GetValue(serializableObject);
                    writer.SetValue(fieldValue, Info.FieldType);
                }
                else
                {
                    SerializationMethodInfo.Invoke(serializableObject, new object[] { writer });
                }
            }
            public void Read(IAutoSerializable serializableObject, BasicReader reader)
            {
                if (DeserializationMethodInfo == null)
                {
                    Info.SetValue(serializableObject, reader.GetValue(Info.FieldType));
                }
                else
                {
                    DeserializationMethodInfo.Invoke(serializableObject, new object[] { reader });
                }
            }
        }

        internal static bool IsRuntimeCallOverriden { get; private set; }

        private static Dictionary<Type, SerializableFieldMemory[]> _fields;
        private static Dictionary<string, BasicCommand> _commands;
        private static Dictionary<ushort, Type> _messageTypes;
        private static Dictionary<Type, NetworkMessageCaller> _messageHookers;
        private static Dictionary<string, IPlugin> _plugins;

        private static DirectoryInfo _pluginDirInfo;

        static ReflectionProvider()
        {
            _fields = new Dictionary<Type, SerializableFieldMemory[]>();
            _commands = new Dictionary<string, BasicCommand>();
            _messageTypes = new Dictionary<ushort, Type>();
            _messageHookers = new Dictionary<Type, NetworkMessageCaller>();
            _pluginDirInfo = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins"));
            _plugins = new Dictionary<string, IPlugin>();

            RegisterAll();
            InitializePlugins();
        }

        internal static bool Invalidate() => true;

        internal static BasicCommand[] GetAllCommands()
        {
            lock (_commands)
            {
                return _commands.Values.ToArray();
            }
        }
        internal static IPlugin GetPluginByIdentifier(string identifier)
        {
            _plugins.TryGetValue(identifier, out var plugin);
            return plugin;
        }

        internal static bool TryGetFields(Type type, out SerializableFieldMemory[] fields)
        {
            return _fields.TryGetValue(type, out fields);
        }
        internal static bool TryGetCommand(string commandName, out BasicCommand command)
        {
            return _commands.TryGetValue(commandName, out command);
        }
        internal static bool TryGetMessageType(ushort messageId, out Type messageType)
        {
            return _messageTypes.TryGetValue(messageId, out messageType);
        }
        internal static bool TryGetMessageHooker(Type receiverType, out NetworkMessageCaller caller)
        {
            return _messageHookers.TryGetValue(receiverType, out caller);
        }

        private static void RegisterAll()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            try
            {
                foreach (var assembly in assemblies)
                {
                    var types = assembly.GetTypes()
                        .Where((t) => t.IsClass && !t.IsAbstract);

                    foreach (var type in types)
                    {
                        if (!IsRuntimeCallOverriden && type.GetCustomAttribute<OverrideRuntimeCall>() != null)
                        {
                            IsRuntimeCallOverriden = true;
                        }

                        if (type.IsSubclassOf(typeof(BasicCommand)))
                        {
                            var instance = (BasicCommand)Activator.CreateInstance(type);
                            if (!_commands.ContainsKey(instance.Name))
                            {
                                _commands.Add(instance.Name, instance);
                            }
                        }

                        if (type.IsSubclassOf(typeof(NetworkMessage)))
                        {
                            var message = NetworkProtocol.CreateMessage(type);

                            if (!_messageTypes.ContainsKey(message.MessageId))
                            {
                                _messageTypes.Add(message.MessageId, type);
                            }
                        }
                        else if (typeof(IMessageHooker).IsAssignableFrom(type))
                        {
                            var sMethods = type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic);
                            if (sMethods.Length > 0)
                            {
                                foreach (var smethod in sMethods)
                                {
                                    var ps = smethod.GetParameters();
                                    if (ps.Length >= 2 && ps[0].ParameterType.IsSubclassOf(typeof(NetworkMessage)) && (ps[1].ParameterType.IsSubclassOf(typeof(NetworkClientEntity)) || ps[1].ParameterType.IsSubclassOf(typeof(NetworkConnectionEntity))))
                                    {
                                        var msgType = ps[0].ParameterType;
                                        var entityType = ps[1].ParameterType;

                                        if (!_messageHookers.ContainsKey(entityType))
                                        {
                                            _messageHookers.Add(entityType, new NetworkMessageCaller());
                                        }

                                        _messageHookers[entityType].RegisterReference(msgType, smethod);
                                    }
                                }
                            }
                        }

                        if (typeof(IAutoSerializable).IsAssignableFrom(type))
                        {
                            var memoryList = new List<SerializableFieldMemory>();
                            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance).OrderBy((field) => field.Name);
                            foreach (var field in fields)
                            {
                                if (field.GetCustomAttribute<IgnoreInProcess>() != null) continue;

                                memoryList.Add(new SerializableFieldMemory(field));
                            }

                            _fields.Add(type, memoryList.ToArray());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new CriticalException("An error occured during reflection registration", ex);
            }
        }
        private static void InitializePlugins()
        {
            if (!_pluginDirInfo.Exists)
            {
                _pluginDirInfo.Create();
            }

            var dllInfos = _pluginDirInfo.GetFiles()
                .Where((fileInfo) => fileInfo.Extension.Equals(".dll"));

            foreach (var dllInfo in dllInfos)
            {
                var assembly = Assembly.LoadFrom(dllInfo.FullName);
                var plugins = assembly.GetTypes()
                    .Where((type) => typeof(IPlugin).IsAssignableFrom(type));

                foreach (var plugin in plugins)
                {
                    IPlugin? instance = null;

                    try
                    {
                        instance = (IPlugin)Activator.CreateInstance(plugin);
                        instance.OnInitialize();

                        if (_plugins.ContainsKey(instance.Identifier))
                        {
                            throw new FriendlyException($"A plugin with the same identifier ({instance.Identifier}) is already registered");
                        }

                        _plugins.Add(instance.Identifier, instance);

                        if (instance.AutoExecute)
                        {
                            Run.Plugin(instance.Identifier);
                        }
                    }
                    catch (Exception ex)
                    {
                        Run.OnPluginLoadFailed(instance?.Identifier, ex);
                    }
                }
            }
        }

    }
}