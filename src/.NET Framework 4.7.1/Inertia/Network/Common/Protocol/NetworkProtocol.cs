using System;
using System.Collections.Generic;
using System.Reflection;

namespace Inertia.Network
{
    public abstract class NetworkProtocol
    {
        private static NetworkProtocol _current;
        private static Dictionary<uint, Type> _messageTypes;
        private static Dictionary<Type, NetworkMessageCaller> _messageHookers;
        
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

        public static NetworkMessage CreateMessage<T>() where T : NetworkMessage
        {
            return CreateMessage(typeof(T));
        }
        public static NetworkMessage CreateMessage(uint messageId)
        {
            if (_messageTypes.ContainsKey(messageId))
            {
                return CreateMessage(_messageTypes[messageId]);
            }

            return null;
        }
        public static NetworkMessage CreateMessage(Type messageType)
        {
            if (messageType.IsAbstract || !messageType.IsSubclassOf(typeof(NetworkMessage)))
            {
                return null;
            }

            var constr = messageType.GetConstructors()[0];
            var parameters = constr.GetParameters();
            var objs = new object[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
            {
                objs[i] = null;
            }

            return (NetworkMessage)constr.Invoke(objs);
        }

        internal static void ProcessParsing(object receiver, BasicReader reader)
        {
            var output = new MessageParsingOutput();
            var caller = GetCaller(receiver);

            _current.OnParseMessage(receiver, reader, output);
            
            if (caller != null)
            {
                foreach (var message in output.Messages)
                {
                    if (_current.PooledExecution)
                    {
                        _current.AsyncQueue.Enqueue(() => caller.CallReferences(message, receiver));
                    }
                    else
                    {
                        caller.CallReferences(message, receiver);
                    }
                }
            }

            output
                .SetAsReaded()
                .Dispose();
        }

        public abstract bool PooledExecution { get; }
        /// <summary>
        /// The size of the buffer to be used for network communication.
        /// </summary>
        public abstract int NetworkBufferLength { get; }

        private protected AutoQueueExecutor AsyncQueue { get; private set; }

        static NetworkProtocol()
        {
            _messageTypes = new Dictionary<uint, Type>();
            _messageHookers = new Dictionary<Type, NetworkMessageCaller>();

            var assemblys = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblys)
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (!type.IsClass || type.IsAbstract)
                    {
                        continue;
                    }

                    if (type.IsSubclassOf(typeof(NetworkMessage)))
                    {
                        var message = CreateMessage(type);

                        if (!_messageTypes.ContainsKey(message.MessageId))
                        {
                            _messageTypes.Add(message.MessageId, type);
                            VerifyReceivers(type);
                        }
                    }
                    else if (type.IsSealed && typeof(IMessageHooker).IsAssignableFrom(type))
                    {
                        VerifyReceivers(type);
                    }
                }
            }

            _current = new DefaultNetworkProtocol();

            void VerifyReceivers(Type type)
            {
                var sMethods = type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic);
                if (sMethods.Length > 0)
                {
                    foreach (var smethod in sMethods)
                    {
                        var ps = smethod.GetParameters();
                        if (ps.Length >= 2 && ps[0].ParameterType.IsSubclassOf(typeof(NetworkMessage)) && (ps[1].ParameterType.IsSubclassOf(typeof(NetworkClientEntity)) || ps[1].ParameterType.IsSubclassOf(typeof(NetworkConnectionEntity))))
                        {
                            var msgType = ps[0].ParameterType;
                            var entityType = ps[1].ParameterType;
                            if (!_messageHookers.ContainsKey(entityType))
                            {
                                _messageHookers.Add(entityType, new NetworkMessageCaller());
                            }

                            _messageHookers[entityType].RegisterReference(msgType, smethod);
                        }
                    }
                }
            }
        }
        protected NetworkProtocol()
        {
            if (PooledExecution)
            {
                AsyncQueue = new AutoQueueExecutor();
            }
        }

        /// <summary>
        /// Occurs when a <see cref="NetworkMessage"/> is requested to be written before being sent.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public abstract byte[] OnSerializeMessage(NetworkMessage message);
        public abstract void OnParseMessage(object receiver, BasicReader reader, MessageParsingOutput output);
    }
}
