using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Inertia.Network
{
    public abstract class NetworkProtocol
    {
        internal static ServerMessagePoolExecutor ServerAsyncPool;
        public static NetworkProtocol UsedProtocol { get; private set; }

        internal static void ProcessParsing(object receiver, BasicReader reader)
        {
            var output = new MessageParsingOutput();
            var caller = GetCaller(receiver);
            
            UsedProtocol.OnParseMessage(receiver, reader, output);

            if (output.Messages.Count > 0 && caller != null)
            {
                if (receiver is NetworkConnectionEntity connection)
                {
                    connection.AssignedMessageQueue.Enqueue(ExecuteOutput);
                }
                else
                {
                    NetworkClientEntity.ClientAsyncPool.Enqueue(ExecuteOutput);
                }
            }
            else
            {
                output.Clean();
            }

            void ExecuteOutput()
            {
                foreach (var message in output.Messages)
                {
                    caller.TryCallReference(message, receiver);
                }

                output.Clean();
            }
        }

        public static void SetProtocol(NetworkProtocol protocol)
        {
            UsedProtocol = protocol;
            ServerAsyncPool.ConnectionPerQueue = UsedProtocol.ConnectionPerQueueInPool;
        }
        public static T CreateMessage<T>() where T : NetworkMessage
        {
            return (T)CreateMessage(typeof(T));
        }
        public static NetworkMessage? CreateMessage(ushort messageId)
        {
            if (ReflectionProvider.TryGetMessageType(messageId, out var messageType))
            {
                return CreateMessage(messageType);
            }

            return null;
        }
        public static NetworkMessage CreateMessage(Type messageType)
        {
            if (messageType.IsAbstract || !messageType.IsSubclassOf(typeof(NetworkMessage))) return null;

            var constr = messageType.GetConstructors()[0];
            var parameters = constr
                .GetParameters()
                .Select(p => (object)null)
                .ToArray();

            return (NetworkMessage)constr.Invoke(parameters);
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
        public abstract int AuthorizedDataCountPerSecond { get; }

        static NetworkProtocol()
        {
            UsedProtocol = new DefaultNetworkProtocol();
            ServerAsyncPool = new ServerMessagePoolExecutor(UsedProtocol.ConnectionPerQueueInPool);

            ReflectionProvider.Invalidate();
        }
        protected NetworkProtocol()
        {
        }

        /// <summary>
        /// Occurs when a <see cref="NetworkMessage"/> is requested to be written before being sent.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public abstract byte[] OnSerializeMessage(NetworkMessage message);
        public abstract void OnParseMessage(object receiver, BasicReader reader, MessageParsingOutput output);
        public abstract void OnParsingError(object receiver, Exception ex);
    }
}