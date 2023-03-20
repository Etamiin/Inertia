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

            var caller = GetCaller(receiver);
            if (caller == null) return;

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
                    caller.TryCallReference(message, receiver);
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

        protected static NetworkMessage? CreateMessage(ushort messageId)
        {
            if (ReflectionProvider.TryGetMessageType(messageId, out var messageType))
            {
                return CreateMessage(messageType);
            }

            return null;
        }
        /// <summary>
        /// Returns the instance of <see cref="NetworkMessageCaller"/> associated with the indicated <see cref="NetworkMessage"/> or null.
        /// </summary>
        /// <param name="receiver"></param>
        protected static NetworkMessageCaller GetCaller(object receiver)
        {
            ReflectionProvider.TryGetMessageHooker(receiver.GetType(), out NetworkMessageCaller caller);
            return caller;
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