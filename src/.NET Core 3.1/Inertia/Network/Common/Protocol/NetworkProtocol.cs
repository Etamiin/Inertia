using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Inertia.Network
{
    public abstract class NetworkProtocol
    {
        private static NetworkProtocol _current;
        private static Dictionary<ushort, Type> _messageTypes;
        private static Dictionary<Type, NetworkMessageCaller> _messageHookers;

        internal static bool TryRegisterMessage(Type type)
        {
            var message = CreateMessage(type);

            if (!_messageTypes.ContainsKey(message.MessageId))
            {
                _messageTypes.Add(message.MessageId, type);
                return true;
            }

            return false;
        }
        internal static NetworkMessageCaller RegisterHooker(Type type)
        {
            if (!_messageHookers.ContainsKey(type))
            {
                _messageHookers.Add(type, new NetworkMessageCaller());
            }

            return _messageHookers[type];
        }
        internal static void ProcessParsing(object receiver, BasicReader reader)
        {
            var output = new MessageParsingOutput();
            var caller = GetCaller(receiver);

            var canBeParsed = _current.OnParseMessage(receiver, reader, output);
            if (canBeParsed && caller != null)
            {
                foreach (var message in output.Messages)
                {
                    if (_current.PooledExecution)
                    {
                        _current.AsyncPool.Enqueue(() => caller.CallReference(message, receiver));
                    }
                    else
                    {
                        caller.CallReference(message, receiver);
                    }
                }
            }

            output
                .SetAsReaded()
                .Dispose();
        }

        public static NetworkProtocol GetCurrentProtocol()
        {
            return _current;
        }
        public static void SetProtocol(NetworkProtocol protocol)
        {
            _current = protocol;
        }
        /// <summary>
        /// Returns the instance of <see cref="NetworkMessageCaller"/> associated with the indicated <see cref="NetworkMessage"/> or null.
        /// </summary>
        /// <param name="receiver"></param>
        protected static NetworkMessageCaller GetCaller(object receiver)
        {
            _messageHookers.TryGetValue(receiver.GetType(), out NetworkMessageCaller caller);
            return caller;
        }

        public static T CreateMessage<T>() where T : NetworkMessage
        {
            return (T)CreateMessage(typeof(T));
        }
        public static NetworkMessage CreateMessage(ushort messageId)
        {
            if (_messageTypes.TryGetValue(messageId, out Type messageType))
            {
                return CreateMessage(messageType);
            }

            return null;
        }
        public static NetworkMessage CreateMessage(Type messageType)
        {
            if (messageType.IsAbstract || !messageType.IsSubclassOf(typeof(NetworkMessage))) return null;

            var constr = messageType.GetConstructors()[0];
            var parameters = constr.GetParameters();
            
            return (NetworkMessage)constr.Invoke(new object[parameters.Length]);
        }

        public abstract bool PooledExecution { get; }
        /// <summary>
        /// The size of the buffer to be used for network communication.
        /// </summary>
        public abstract int NetworkBufferLength { get; }

        private protected ExecutorPool AsyncPool { get; private set; }

        static NetworkProtocol()
        {
            _messageTypes = new Dictionary<ushort, Type>();
            _messageHookers = new Dictionary<Type, NetworkMessageCaller>();
            _current = new DefaultNetworkProtocol();
        }
        protected NetworkProtocol()
        {
            if (PooledExecution)
            {
                AsyncPool = new ExecutorPool(100, true);
            }
        }

        /// <summary>
        /// Occurs when a <see cref="NetworkMessage"/> is requested to be written before being sent.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public abstract byte[] OnSerializeMessage(NetworkMessage message);
        public abstract bool OnParseMessage(object receiver, BasicReader reader, MessageParsingOutput output);
    }
}
