using Inertia.Logging;
using System;
using System.Linq;

namespace Inertia.Network
{
    public static class NetworkProtocolFactory
    {
        internal const int DefaultBacklogSize = 1000;

        internal static AsyncExecutionQueuePool? ClientExecutionPool { get; private set; }
        internal static ServerMessagePoolExecutor? ServerAsyncPool { get; private set; }
        internal static NetworkProtocol DefaultProtocol { get; private set; }
        internal static DefaultWebSocketNetworkProtocol DefaultWsProtocol { get; private set; }

        public static NetworkProtocol CurrentProtocol { get; private set; }

        public static void ExecuteClientExecutionPool()
        {
            if (ReflectionProvider.IsAsyncPoolDisabled)
            {
                ClientExecutionPool?.Execute();
            }
        }

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
                    .Select((p) => (object)null)
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
            switch (protocolType)
            {
                case DefaultNetworkProtocols.Default:
                    SetDefaultProtocol(DefaultProtocol);
                    break;
                case DefaultNetworkProtocols.WebSocket:
                    SetDefaultProtocol(DefaultWsProtocol);
                    break;
            }
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

            if (ClientExecutionPool == null)
            {
                if (ReflectionProvider.IsNetworkClientUsedInAssemblies)
                {
                    ClientExecutionPool = new AsyncExecutionQueuePool(protocol.ClientMessagePerQueueCapacity, false);
                }
            }
            else
            {
                ClientExecutionPool.QueueCapacity = protocol.ClientMessagePerQueueCapacity;
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
                ClientExecutionPool?.Enqueue(ExecuteOutput);
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