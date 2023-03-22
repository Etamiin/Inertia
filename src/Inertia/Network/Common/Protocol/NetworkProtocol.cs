using Inertia.Logging;
using System;
using System.Linq;

namespace Inertia.Network
{
    public abstract class NetworkProtocol
    {
        public static NetworkProtocol Current { get; private set; }

        public static void SetProtocol(NetworkProtocol protocol)
        {
            if (Current == protocol) return;

            Current = protocol;
            if (ServerAsyncPool != null)
            {
                ServerAsyncPool.ConnectionPerQueue = Current.ConnectionPerQueueInPool;
            }
            if (ClientExecutionPool != null)
            {
                ClientExecutionPool.QueueCapacity = Current.ClientMessagePerQueueCapacity;
            }
        }

        internal static AsyncExecutionQueuePool? ClientExecutionPool { get; private set; }
        internal static ServerMessagePoolExecutor? ServerAsyncPool { get; private set; }

        internal static void ProcessParsing(object receiver, BasicReader reader)
        {
            var output = new MessageParsingOutput();

            if (!Current.ParseMessage(receiver, reader, output)) return;
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
            else //client entity
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
        internal static NetworkMessage CreateMessage(Type messageType)
        {
            if (messageType.IsAbstract || !messageType.IsSubclassOf(typeof(NetworkMessage))) return null;

            var cnstr = messageType.GetConstructor(Type.EmptyTypes);
            if (cnstr == null)
            {
                cnstr = messageType.GetConstructors()[0];
                var parameters = cnstr
                    .GetParameters()
                    .Select((p) => (object)null)
                    .ToArray();

                return (NetworkMessage)cnstr.Invoke(parameters);
            }
            else
            {
                return (NetworkMessage)Activator.CreateInstance(messageType);
            }
        }
        internal static bool TryGetHandler(object receiver, out NetworkMessageHandler handler)
        {
            return ReflectionProvider.TryGetMessageHandler(receiver.GetType(), out handler);
        }

        protected static NetworkMessage? CreateMessage(ushort messageId)
        {
            if (ReflectionProvider.TryGetMessageType(messageId, out var messageType))
            {
                return CreateMessage(messageType);
            }

            return null;
        }

        /// <summary>
        /// The size of the buffer to be used for network communication.
        /// </summary>
        public abstract int NetworkBufferLength { get; }
        public abstract int ConnectionPerQueueInPool { get; }
        public abstract int ClientMessagePerQueueCapacity { get; }
        public abstract int MaximumMessageCountPerSecond { get; }

        static NetworkProtocol()
        {
            Current = new DefaultNetworkProtocol();

            if (ReflectionProvider.NetworkClientUsedInAssemblies)
            {
                ClientExecutionPool = new AsyncExecutionQueuePool(Current.ClientMessagePerQueueCapacity);
            }

            if (ReflectionProvider.NetworkServerUsedInAssemblies)
            {
                ServerAsyncPool = new ServerMessagePoolExecutor(Current.ConnectionPerQueueInPool);
            }
        }
        protected NetworkProtocol()
        {
        }

        /// <summary>
        /// Occurs when a <see cref="NetworkMessage"/> is requested to be written before being sent.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public abstract byte[] SerializeMessage(NetworkMessage message);
        public abstract bool ParseMessage(object receiver, BasicReader reader, MessageParsingOutput output);
    }
}