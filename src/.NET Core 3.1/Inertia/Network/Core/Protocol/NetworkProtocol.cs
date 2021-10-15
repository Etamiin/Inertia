using System;
using System.Collections.Generic;
using System.Reflection;

namespace Inertia.Network
{
    public abstract class NetworkProtocol
    {
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
        }

        public static NetworkMessage CreateMessage<T>() where T : NetworkMessage
        {
            return CreateMessage(typeof(T));
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
                        var isValidMessage = type.IsClass && type.IsSubclassOf(typeof(NetworkMessage)) && !type.IsAbstract;
                        if (isValidMessage)
                        {
                            var message = CreateMessage(type);

                            if (!_messageTypes.ContainsKey(message.MessageId))
                            {
                                _messageTypes.Add(message.MessageId, type);

                                var sMethods = type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic);
                                if (sMethods.Length > 0)
                                {
                                    foreach (var smethod in sMethods)
                                    {
                                        var ps = smethod.GetParameters();
                                        if (ps.Length >= 2 && ps[0].ParameterType.IsSubclassOf(typeof(NetworkMessage)) && (ps[1].ParameterType.IsSubclassOf(typeof(NetworkClientEntity)) || ps[1].ParameterType.IsSubclassOf(typeof(NetworkConnectionEntity))))
                                        {
                                            var msgType = ps[0].ParameterType;

                                            if (!_messageHookers.ContainsKey(msgType))
                                            {
                                                _messageHookers.Add(msgType, new NetworkMessageCaller());
                                            }

                                            _messageHookers[msgType].RegisterReference(smethod, ps[1].ParameterType);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Occurs when a <see cref="NetworkMessage"/> is requested to be written before being sent.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
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
    }
}
