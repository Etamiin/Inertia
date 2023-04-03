using Inertia.Logging;
using System;
using System.Linq;

namespace Inertia.Network
{
    public static class NetworkProtocolFactory
    {
        internal const int DefaultBacklogSize = 1000;
        internal const int DefaultMessageCountLimitBeforeSpam = 55;

        internal static ServerMessagePoolExecutor? ServerAsyncPool { get; private set; }
        internal static NetworkProtocol DefaultProtocol { get; private set; }
        internal static DefaultWebSocketNetworkProtocol DefaultWsProtocol { get; private set; }

        public static NetworkProtocol CurrentProtocol { get; private set; }

        static NetworkProtocolFactory()
        {
            DefaultProtocol = new DefaultNetworkProtocol();
            DefaultWsProtocol = new DefaultWebSocketNetworkProtocol();

            SetDefaultProtocol(DefaultNetworkProtocols.Default);
        }

        internal static NetworkMessage CreateMessage(Type messageType)
        {
            if (messageType.IsAbstract || !messageType.IsSubclassOf(typeof(NetworkMessage))) return null;

            var cnstr = messageType.GetConstructor(Type.EmptyTypes);
            if (cnstr != null)
            {
                return (NetworkMessage)cnstr.Invoke(new object[0]);
            }
            else
            {
                cnstr = messageType.GetConstructors()[0];
                var parameters = cnstr
                    .GetParameters()
                    .Select((_) => (object)null)
                    .ToArray();

                return (NetworkMessage)cnstr.Invoke(parameters);
            }
        }
        internal static bool TryGetHandler(INetworkEntity receiver, out NetworkMessageHandler handler)
        {
            return ReflectionProvider.TryGetMessageHandler(receiver.GetType(), out handler);
        }

        public static void SetDefaultProtocol(DefaultNetworkProtocols protocolType)
        {
            if (protocolType == DefaultNetworkProtocols.Default) SetDefaultProtocol(DefaultProtocol);
            else if (protocolType == DefaultNetworkProtocols.WebSocket) SetDefaultProtocol(DefaultWsProtocol);
        }
        public static void SetDefaultProtocol(NetworkProtocol protocol)
        {
            if (CurrentProtocol == protocol) return;

            CurrentProtocol = protocol;
            if (ServerAsyncPool == null)
            {
                if (ReflectionProvider.IsNetworkServerUsedInAssemblies)
                {
                    ServerAsyncPool = new ServerMessagePoolExecutor(protocol.ConnectionPerQueueInPool);
                }
            }
            else
            {
                ServerAsyncPool.ConnectionPerQueue = protocol.ConnectionPerQueueInPool;
            }
        }

        public static void ProcessParsing(NetworkProtocol protocol, INetworkEntity receiver, BasicReader reader)
        {
            var output = new MessageParsingOutput();

            if (!protocol.ParseMessage(receiver, reader, output)) return;
            if (output.Messages.Count == 0)
            {
                output.Dispose();
                return;
            }

            if (!TryGetHandler(receiver, out var handler)) return;

            if (receiver is NetworkConnectionEntity connection)
            {
                connection.AssignedMessageQueue.Enqueue(ExecuteOutput);
            }
            else
            {
                var client = receiver as NetworkClientEntity;
                if (client.Parameters.ExecutionPool == null)
                {
                    SimpleLogger.Default.Error($"Cannot process '{receiver.GetType().Name}' messages: ExecutionPool is null.");
                    
                    return;
                }

                client.Parameters.ExecutionPool.Enqueue(ExecuteOutput);
            }

            void ExecuteOutput()
            {
                foreach (var message in output.Messages)
                {
                    handler.TryHandle(message, receiver);
                }

                output.Dispose();
            }
        }
    }
}