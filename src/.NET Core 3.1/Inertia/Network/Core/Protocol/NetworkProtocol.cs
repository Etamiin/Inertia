using System;
using System.Collections.Generic;
using System.Reflection;

namespace Inertia.Network
{
    public abstract class NetworkProtocol
    {
<<<<<<< HEAD
        /// <summary>
        /// Set to true for executing the tasks in multi threading
        /// </summary>
        public static bool MultiThreadedExecution { get; set; } = true;
        /// <summary>
        /// The size of the buffer to be used for network communication.
        /// </summary>
        public static int NetworkBufferLength { get; set; } = 8192;

        public static NetworkProtocol GetProtocol()
        {
            return _protocol;
        }
        public static void SetProtocol(NetworkProtocol protocol)
        {
            _protocol = protocol;
=======
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
>>>>>>> premaster
        }

        public static NetworkMessage CreateMessage<T>() where T : NetworkMessage
        {
            return CreateMessage(typeof(T));
        }
<<<<<<< HEAD
=======
        public static NetworkMessage CreateMessage(uint messageId)
        {
            if (_messageTypes.ContainsKey(messageId))
            {
                return CreateMessage(_messageTypes[messageId]);
            }

            return null;
        }
>>>>>>> premaster
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
<<<<<<< HEAD
        public static NetworkMessage CreateMessage(uint messageId)
        {
            if (!_isDataLoaded)
            {
                SetProtocol(new DefaultNetworkProtocol());
            }

            if (_messageTypes.ContainsKey(messageId))
            {
                return CreateMessage(_messageTypes[messageId]);
            }

            return null;
        }

        /// <summary>
        /// Returns the instance of <see cref="NetworkMessageCaller"/> associated with the indicated <see cref="NetworkMessage"/> or null.
        /// </summary>
        /// <param name="message"></param>
        protected static NetworkMessageCaller GetCaller(NetworkMessage message)
        {
            if (!_isDataLoaded)
            {
                SetProtocol(new DefaultNetworkProtocol());
            }

            _messageHookers.TryGetValue(message.GetType(), out NetworkMessageCaller caller);
            return caller;
        }

        private static Dictionary<uint, Type> _messageTypes;
        private static Dictionary<Type, NetworkMessageCaller> _messageHookers;        
        private static NetworkProtocol _protocol;

        public virtual ushort ProtocolVersion { get; }

        private static bool _isDataLoaded => _messageTypes != null;

        protected NetworkProtocol()
        {
            if (!_isDataLoaded)
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
                        else if (type.IsSubclassOf(typeof(NetworkMessageHandler)))
                        {
                            VerifyReceivers(type);
                        }
                    }
                }
            }

=======

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

>>>>>>> premaster
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
<<<<<<< HEAD

                            if (!_messageHookers.ContainsKey(msgType))
                            {
                                _messageHookers.Add(msgType, new NetworkMessageCaller());
                            }

                            _messageHookers[msgType].RegisterReference(smethod, ps[1].ParameterType);
=======
                            var entityType = ps[1].ParameterType;
                            if (!_messageHookers.ContainsKey(entityType))
                            {
                                _messageHookers.Add(entityType, new NetworkMessageCaller());
                            }

                            _messageHookers[entityType].RegisterReference(msgType, smethod);
>>>>>>> premaster
                        }
                    }
                }
            }
        }
<<<<<<< HEAD
=======
        protected NetworkProtocol()
        {
            if (PooledExecution)
            {
                AsyncQueue = new AutoQueueExecutor();
            }
        }
>>>>>>> premaster

        /// <summary>
        /// Occurs when a <see cref="NetworkMessage"/> is requested to be written before being sent.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
<<<<<<< HEAD
        public virtual byte[] OnParseMessage(NetworkMessage message)
        {
            return DefaultNetworkProtocol.Instance.OnParseMessage(message);
        }

        /// <summary>
        /// Occurs when data is received from a <see cref="TcpClientEntity"/>.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="reader"></param>
        public virtual void OnReceiveData(TcpClientEntity client, BasicReader reader)
        {
            DefaultNetworkProtocol.Instance.OnReceiveData(client, reader);
        }
        /// <summary>
        /// Occurs when data is received from a <see cref="UdpClientEntity"/>.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="reader"></param>
        public virtual void OnReceiveData(UdpClientEntity client, BasicReader reader)
        {
            DefaultNetworkProtocol.Instance.OnReceiveData(client, reader);
        }
        /// <summary>
        /// Occurs when data is received from a <see cref="TcpConnectionEntity"/>.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="reader"></param>
        public virtual void OnReceiveData(TcpConnectionEntity connection, BasicReader reader)
        {
            DefaultNetworkProtocol.Instance.OnReceiveData(connection, reader);
        }
        /// <summary>
        /// Occurs when data is received from a <see cref="UdpConnectionEntity"/>.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="reader"></param>
        public virtual void OnReceiveData(UdpConnectionEntity connection, BasicReader reader)
        {
            DefaultNetworkProtocol.Instance.OnReceiveData(connection, reader);
        }
=======
        public abstract byte[] OnSerializeMessage(NetworkMessage message);
        public abstract void OnParseMessage(object receiver, BasicReader reader, MessageParsingOutput output);
>>>>>>> premaster
    }
}
