using Inertia.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Inertia.Network
{
    public static class NetworkManager
    {
        private static readonly Dictionary<ushort, Type> _messageTypes;
        private static readonly Dictionary<Type, Type> _indirectEntityTypes;
        private static readonly Dictionary<Type, NetworkMessageHandlerInvoker> _handlerInvokerPerEntity;
        
        static NetworkManager()
        {
            _messageTypes = new Dictionary<ushort, Type>();
            _indirectEntityTypes = new Dictionary<Type, Type>();
            _handlerInvokerPerEntity = new Dictionary<Type, NetworkMessageHandlerInvoker>();

            var defaultProtocol = new DefaultNetworkProtocol();

            TcpProtocol = defaultProtocol;
            UdpProtocol = defaultProtocol;
            WsProtocol = new DefaultWebSocketNetworkProtocol();

            LoadAllTypes();
        }

        public static NetworkProtocol TcpProtocol { get; set; }
        public static NetworkProtocol UdpProtocol { get; set; }
        public static WebSocketNetworkProtocol WsProtocol { get; set; }

        public static bool TryGetMessageType(ushort messageId, out Type messageType)
        {
            return _messageTypes.TryGetValue(messageId, out messageType);
        }
        public static void ParseAndHandle(NetworkEntity receiver, DataReader reader)
        {
            if (receiver is null || reader is null) return;

            var output = new MessageParsingOutput();

            if (!receiver.NetworkProtocol.TryDeserializeMessage(receiver, reader, output)) return;
            if (!output.Messages.Any()) return;

            if (_handlerInvokerPerEntity.TryGetValue(receiver.GetType(), out var handlerInvoker))
            {
                foreach (var message in output.Messages)
                {
                    handlerInvoker.TryInvokeHandleMethod(message, receiver);
                }
            }        

            output.Dispose();
        }

        private static void LoadAllTypes()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var types = assembly.GetTypes()
                    .Where((t) => t.IsClass && !t.IsAbstract);

                foreach (var type in types)
                {
                    RegisterMessageType(type);
                    RegisterIndirectEntityType(type);
                    RegisterMessageHandlers(type);
                }
            }

            LinkIndirectHandlers();
        }
        private static void RegisterMessageType(Type type)
        {
            if (type.IsSubclassOf(typeof(NetworkMessage)))
            {
                var message = type.InvokeConstructor<NetworkMessage>();
                if (_messageTypes.ContainsKey(message.MessageId))
                {
                    throw new InvalidOperationException($"Message ID '{message.MessageId}' is already registered.");
                }

                _messageTypes[message.MessageId] = type;
            }
        }
        private static void RegisterIndirectEntityType(Type type)
        {
            if (type.IsSubclassOf(typeof(NetworkEntity)))
            {
                var indirectEntityType = type
                    .GetInterfaces()
                    .FirstOrDefault((interfaceType) => interfaceType.GetCustomAttribute<IndirectNetworkEntityAttribute>() != null);

                if (indirectEntityType != null)
                {
                    _indirectEntityTypes.Add(type, indirectEntityType);
                }
            }
        }
        private static void RegisterMessageHandlers(Type type)
        {
            if (type.GetCustomAttribute<MessageHandlerAttribute>() is null) return;

            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);

            foreach (var method in methods)
            {
                var handlerArgs = method.GetParameters();
                if (handlerArgs.Length != 2) continue;

                var messageType = handlerArgs[0].ParameterType;
                if (!messageType.IsSubclassOf(typeof(NetworkMessage))) continue;

                var entityType = handlerArgs[1].ParameterType;
                var isValidEntityType =
                    entityType.IsSubclassOf(typeof(ClientEntity)) ||
                    entityType.IsSubclassOf(typeof(TcpConnectionEntity)) ||
                    entityType.GetCustomAttribute<IndirectNetworkEntityAttribute>() != null;

                if (!isValidEntityType) continue;

                if (!_handlerInvokerPerEntity.TryGetValue(entityType, out var handlerInvoker))
                {
                    handlerInvoker = new NetworkMessageHandlerInvoker();
                    _handlerInvokerPerEntity.Add(entityType, handlerInvoker);
                }

                var messageParam = Expression.Parameter(typeof(NetworkMessage), "message");
                var entityParam = Expression.Parameter(typeof(NetworkEntity), "entity");
                var callExpr = Expression.Call(
                    Expression.New(type),
                    method,
                    Expression.Convert(messageParam, messageType),
                    Expression.Convert(entityParam, entityType));

                var callback = Expression.Lambda<Action<NetworkMessage, NetworkEntity>>(callExpr, messageParam, entityParam).Compile();

                handlerInvoker.Register(messageType, callback);
            }
        }
        private static void LinkIndirectHandlers()
        {
            foreach (var kvp in _indirectEntityTypes)
            {
                var directEntityType = kvp.Key;
                var indirectEntityType = kvp.Value;
                var invoker = new NetworkMessageHandlerInvoker();

                if (_handlerInvokerPerEntity.TryGetValue(indirectEntityType, out var indirectTypeInvoker))
                {
                    invoker.Register(indirectTypeInvoker);
                }

                if (_handlerInvokerPerEntity.TryGetValue(directEntityType, out var directTypeInvoker))
                {
                    invoker.Register(directTypeInvoker);
                }

                _handlerInvokerPerEntity[directEntityType] = invoker;
                _handlerInvokerPerEntity[indirectEntityType] = invoker;
            }
        }
    }
}