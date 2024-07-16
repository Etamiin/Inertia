using Inertia.Logging;
using System;
using System.Diagnostics;

namespace Inertia.Network
{
    public static class NetworkProtocolManager
    {
        internal const int DefaultBacklogQueueSize = 1000;
        internal const int DefaultMessageCountLimitBeforeSpam = 55;

        internal static ServerMessagePoolExecutor ServerMessagePool { get; private set; }
        internal static NetworkProtocol DefaultProtocol { get; private set; }
        internal static WebSocketNetworkProtocol DefaultWsProtocol { get; private set; }

        public static NetworkProtocol CurrentProtocol { get; private set; }

        static NetworkProtocolManager()
        {
            DefaultProtocol = new DefaultNetworkProtocol();
            DefaultWsProtocol = new WebSocketNetworkProtocol();

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

        public static bool TryGetHandler(NetworkEntity receiver, out NetworkMessageHandler handler)
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
            if (ServerMessagePool == null)
            {
                if (ReflectionProvider.IsNetworkServerUsed)
                {
                    ServerMessagePool = new ServerMessagePoolExecutor(protocol.ConnectionPerQueueInPool, LoggingProvider.Logger);
                }
            }
            else
            {
                ServerMessagePool.ConnectionPerQueue = protocol.ConnectionPerQueueInPool;
            }
        }
        public static void ProcessParsing(NetworkProtocol protocol, NetworkEntity receiver, DataReader reader)
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