﻿using Inertia.Logging;
using Inertia.Network;
using Inertia.Plugins;
using Inertia.Paper;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Inertia
{
    internal static class ReflectionProvider
    {
        internal class SerializablePropertyMemory
        {
            internal readonly PropertyInfo Info;
            internal readonly MethodInfo? SerializationMethodInfo;
            internal readonly MethodInfo? DeserializationMethodInfo;

            internal SerializablePropertyMemory(PropertyInfo info)
            {
                Info = info;

                var serAttr = info.GetCustomAttribute<PropertySerializationAttribute>();
                if (serAttr != null)
                {
                    SerializationMethodInfo = SearchForMethod(serAttr.SerializationMethodName, typeof(BasicWriter));
                    DeserializationMethodInfo = SearchForMethod(serAttr.DeserializationMethodName, typeof(BasicReader));
                }
            }

            public void Write(IAutoSerializable serializableObject, BasicWriter writer)
            {
                if (SerializationMethodInfo == null)
                {
                    var propertyValue = Info.GetValue(serializableObject);
                    writer.SetValue(propertyValue, Info.PropertyType);
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
                    Info.SetValue(serializableObject, reader.GetValue(Info.PropertyType));
                }
                else
                {
                    DeserializationMethodInfo.Invoke(serializableObject, new object[] { reader });
                }
            }
        
            private MethodInfo? SearchForMethod(string methodName, Type parameterType)
            {
                var method = Info.DeclaringType.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
                if (method != null)
                {
                    var parameters = method.GetParameters();
                    if (parameters.Length == 1 && parameters[0].ParameterType == parameterType) return method;
                }

                return null;
            }
        }

        internal static bool IsPaperOwned { get; private set; }
        internal static bool IsNetworkClientUsed { get; private set; }
        internal static bool IsNetworkServerUsed { get; private set; }
        
        private readonly static Dictionary<Type, SerializablePropertyMemory[]> _properties;
        private readonly static Dictionary<string, BasicCommand> _commands;
        private readonly static Dictionary<ushort, Type> _messageTypes;
        private readonly static Dictionary<Type, NetworkMessageHandler> _messagesHandlers;
        private readonly static Dictionary<string, PluginTrace> _pluginTraces;
        
        static ReflectionProvider()
        {
            _properties = new Dictionary<Type, SerializablePropertyMemory[]>();
            _commands = new Dictionary<string, BasicCommand>();
            _messageTypes = new Dictionary<ushort, Type>();
            _messagesHandlers = new Dictionary<Type, NetworkMessageHandler>();
            _pluginTraces = new Dictionary<string, PluginTrace>();

            RegisterAll();
        }

        internal static bool Invalidate() => true;

        internal static bool TryGetProperties(Type type, out SerializablePropertyMemory[] properties)
        {
            return _properties.TryGetValue(type, out properties);
        }
        internal static IEnumerable<BasicCommand> GetAllCommands()
        {
            lock (_commands)
            {
                return _commands.Values.AsEnumerable();
            }
        }
        internal static bool TryGetCommand(string commandName, out BasicCommand command)
        {
            return _commands.TryGetValue(commandName, out command);
        }
        internal static bool TryGetMessageType(ushort messageId, out Type messageType)
        {
            return _messageTypes.TryGetValue(messageId, out messageType);
        }
        internal static bool TryGetMessageHandler(NetworkEntity receiver, out NetworkMessageHandler handler)
        {
            if (_messagesHandlers.TryGetValue(receiver.GetType(), out handler)) return true;
            if (receiver.WrappedType != null) return _messagesHandlers.TryGetValue(receiver.WrappedType, out handler);

            return false;
        }

        internal static PluginExecutionResult TryStartPlugin(string pluginFilePath, object[] executionParameters)
        {
            if (!File.Exists(pluginFilePath)) return PluginExecutionResult.FileNotFound;

            var assembly = Assembly.LoadFrom(pluginFilePath);
            var pluginTypes = assembly.GetTypes()
                .Where((type) => typeof(IPlugin).IsAssignableFrom(type));

            foreach (var pluginType in pluginTypes)
            {
                var instance = TryCreateInstance<IPlugin>(pluginType, Type.EmptyTypes);
                if (_pluginTraces.ContainsKey(instance.Identifier)) continue;

                if (!instance.UsePaper)
                {
                    var options = instance.LongRun ? TaskCreationOptions.LongRunning : TaskCreationOptions.None;
                    var executionCancelSource = new CancellationTokenSource();

                    Task.Factory.StartNew(
                        () => RunPluginExecution(instance, executionParameters),
                        executionCancelSource.Token,
                        options,
                        TaskScheduler.Default);

                    _pluginTraces.Add(instance.Identifier, new PluginTrace(instance, executionCancelSource));
                }
                else
                {
                    _pluginTraces.Add(instance.Identifier, new PluginTrace(instance));
                    TimedPaper.OnNextTick(() => RunPluginExecution(instance, executionParameters));
                }
            }

            return PluginExecutionResult.Success;
        }
        internal static bool TryStopPlugin(string pluginIdentifier)
        {
            if (_pluginTraces.TryGetValue(pluginIdentifier, out var trace))
            {
                trace.Dispose();
                _pluginTraces.Remove(pluginIdentifier);

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
        private static void RunPluginExecution(IPlugin pluginInstance, object[] executionParameters)
        {
            try
            {
                pluginInstance.Execute(executionParameters);
            }
            catch (Exception ex)
            {
                pluginInstance.Error(ex);

                if (pluginInstance.StopOnCatchedError) TryStopPlugin(pluginInstance.Identifier);
            }
        }
        private static void ReadTypeIOInformations(Type type)
        {
            if (typeof(IAutoSerializable).IsAssignableFrom(type))
            {
                if (type.GetCustomAttribute<IgnoreInReflectionAttribute>() != null) return;

                var memoryList = new List<SerializablePropertyMemory>();
                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .OrderBy((property) => property.Name);

                foreach (var property in properties)
                {
                    if (!property.CanWrite || property.GetCustomAttribute<IgnoreInReflectionAttribute>() != null) continue;

                    memoryList.Add(new SerializablePropertyMemory(property));
                }

                _properties.Add(type, memoryList.ToArray());
            }
        }
        private static void ReadTypeInformations(Type type)
        {
            if (type.IsSubclassOf(typeof(BasicCommand)))
            {
                var instance = TryCreateInstance<BasicCommand>(type, Type.EmptyTypes);
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

                        if (!_messagesHandlers.ContainsKey(entityType))
                        {
                            _messagesHandlers.Add(entityType, new NetworkMessageHandler());
                        }

                        _messagesHandlers[entityType].RegisterReference(msgType, smethod);
                    }
                }
            }
        }
    }
}