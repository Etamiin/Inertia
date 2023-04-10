using System;

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

            SetDefaultProtocol(InternalNetworkProtocolType.Default);
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
                var parameters = cnstr.GetParameters();

                return (NetworkMessage)cnstr.Invoke(new object[parameters.Length]);
            }
        }
        internal static bool TryGetHandler(NetworkEntity receiver, out NetworkMessageHandler handler)
        {
            return ReflectionProvider.TryGetMessageHandler(receiver, out handler);
        }

        public static void SetDefaultProtocol(InternalNetworkProtocolType protocolType)
        {
            if (protocolType == InternalNetworkProtocolType.Default) SetDefaultProtocol(DefaultProtocol);
            else if (protocolType == InternalNetworkProtocolType.WebSocket) SetDefaultProtocol(DefaultWsProtocol);
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

        public static void ProcessParsing(NetworkProtocol protocol, NetworkEntity receiver, BasicReader reader)
        {
            var output = new MessageParsingOutput();

            if (!protocol.ParseMessage(receiver, reader, output)) return;
            if (output.Messages.Count == 0)
            {
                output.Dispose();
                return;
            }

            if (!TryGetHandler(receiver, out var handler)) return;

            receiver.ProcessInQueue(ExecuteOutput);
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