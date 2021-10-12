using System;
using System.Collections.Generic;

namespace Inertia.Network
{
    /// <summary>
    ///
    /// </summary>
    public abstract class NetworkProtocol
    {
        /// <summary>
        /// True (default) if you want the tasks to be executed in multithread otherwise false.
        /// </summary>
        public static bool MultiThreadedExecution { get; set; } = true;
        /// <summary>
        /// The size of the buffer to be used for network communication.
        /// </summary>
        public static int NetworkBufferLength { get; set; } = 8192;

        private static Dictionary<uint, Type> MessageTypes => LoaderManager.NetworkMessageTypes;
        private static Dictionary<Type, NetworkMessageCaller> MessageHookers => LoaderManager.NetworkMessageHookers;

        private static NetworkProtocol _protocol;

        /// <summary>
        /// Returns the instance of the <see cref="NetworkProtocol"/> currently used by the network entities.
        /// </summary>
        /// <returns></returns>
        public static NetworkProtocol GetProtocol()
        {
            return _protocol;
        }
        /// <summary>
        /// Specify the instance of the NetworkProtocol to be used for the network entities.
        /// </summary>
        /// <param name="protocol"><see cref="NetworkProtocol"/></param>
        public static void SetProtocol(NetworkProtocol protocol)
        {
            _protocol = protocol;
        }
        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public static NetworkMessage CreateMessage<T>() where T : NetworkMessage
        {
            return CreateMessage(typeof(T));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageType"></param>
        /// <returns></returns>
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
        /// <summary>
        ///
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        public static NetworkMessage CreateMessage(uint messageId)
        {
            if (MessageTypes.ContainsKey(messageId))
            {
                return CreateMessage(MessageTypes[messageId]);
            }

            return null;            
        }

        /// <summary>
        /// Returns the instance of <see cref="NetworkMessageCaller"/> associated with the indicated <see cref="NetworkMessage"/> or null.
        /// </summary>
        /// <param name="message"></param>
        public static NetworkMessageCaller GetCaller(NetworkMessage message)
        {
            MessageHookers.TryGetValue(message.GetType(), out NetworkMessageCaller caller);
            return caller;
        }

        /// <summary>
        /// Specify the version of the network protocol that can be used.
        /// </summary>
        public virtual ushort ProtocolVersion { get; }

        /// <summary>
        /// 
        /// </summary>
        public NetworkProtocol()
        {
            LoaderManager.DefaultLoadNetwork();
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
