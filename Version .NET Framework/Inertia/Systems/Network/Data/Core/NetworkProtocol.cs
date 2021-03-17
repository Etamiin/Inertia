using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Inertia.Internal;

namespace Inertia.Network
{
    /// <summary>
    /// Represents the class for writing and reading network data
    /// </summary>
    public abstract class NetworkProtocol
    {
        protected static Dictionary<uint, Type> MessageTypes => LoaderManager.NetworkMessageTypes;
        protected static Dictionary<Type, NetworkMessageHookerRefs> MessageHookers => LoaderManager.NetworkMessageHookers;
        
        private static NetworkProtocol _protocol;
        
        /// <summary>
        /// Get the curent <see cref="NetworkProtocol"/> instance used
        /// </summary>
        /// <returns></returns>
        public static NetworkProtocol GetProtocol()
        {
            if (_protocol == null)
                _protocol = DefaultNetworkProtocol.Instance;

            return _protocol;
        }
        /// <summary>
        /// Set a custom <see cref="NetworkProtocol"/> instance to be used
        /// </summary>
        /// <param name="protocol"><see cref="NetworkProtocol"/></param>
        public static void SetProtocol(NetworkProtocol protocol)
        {
            _protocol = protocol;
        }
        /// <summary>
        /// Create an instance of <typeparamref name="T"/>
        /// </summary>
        /// <returns></returns>
        public static NetworkMessage CreateInstance<T>() where T : NetworkMessage
        {
            return CreateInstance(typeof(T));
        }
        /// <summary>
        /// Create an instance of the specified <see cref="Type"/>
        /// </summary>
        /// <param name="packetType"></param>
        /// <returns></returns>
        public static NetworkMessage CreateInstance(Type packetType)
        {
            if (packetType.IsAbstract || !packetType.IsSubclassOf(typeof(NetworkMessage)))
                return null;

            var constr = packetType.GetConstructors()[0];
            var parameters = constr.GetParameters();
            var objs = new object[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
                objs[i] = null;

            return (NetworkMessage)constr.Invoke(objs);
        }
        /// <summary>
        /// Create an instance of a <see cref="NetworkMessage"/> based on his Id
        /// </summary>
        /// <param name="packetId">Id of the target <see cref="NetworkMessage"/></param>
        /// <returns></returns>
        public static NetworkMessage CreateInstance(uint packetId)
        {
            if (!MessageTypes.ContainsKey(packetId))
                return null;

            return CreateInstance(MessageTypes[packetId]);
        }

        /// <summary>
        /// Get a <see cref="NetworkMessageHookerRefs"/> if exist or null
        /// </summary>
        /// <param name="message"></param>
        public static NetworkMessageHookerRefs GetHookerRefs(NetworkMessage message)
        {
            MessageHookers.TryGetValue(message.GetType(), out NetworkMessageHookerRefs refs);
            return refs;
        }

        /// <summary>
        /// Invoke a MessageHooker with the specified parameters
        /// </summary>
        /// <param name="message"></param>
        /// <param name="client"></param>
        public static void CallHookerRef(NetworkMessage message, NetTcpClient client)
        {
            var refs = GetHookerRefs(message);
            if (refs != null)
                refs.CallHookerRef(message, client);
        }
        /// <summary>
        /// Invoke a MessageHooker with the specified parameters
        /// </summary>
        /// <param name="message"></param>
        /// <param name="client"></param>
        public static void CallHookerRef(NetworkMessage message, NetUdpClient client)
        {
            var refs = GetHookerRefs(message);
            if (refs != null)
                refs.CallHookerRef(message, client);
        }
        /// <summary>
        /// Invoke a MessageHooker with the specified parameters
        /// </summary>
        /// <param name="message"></param>
        /// <param name="connection"></param>
        public static void CallHookerRef(NetworkMessage message, NetTcpConnection connection)
        {
            var refs = GetHookerRefs(message);
            if (refs != null)
                refs.CallHookerRef(message, connection);
        }
        /// <summary>
        /// Invoke a MessageHooker with the specified parameters
        /// </summary>
        /// <param name="message"></param>
        /// <param name="connection"></param>
        public static void CallHookerRef(NetworkMessage message, NetUdpConnection connection)
        {
            var refs = GetHookerRefs(message);
            if (refs != null)
                refs.CallHookerRef(message, connection);
        }

        /// <summary>
        /// Represent the protocol version used by the current protocol
        /// </summary>
        public abstract ushort ProtocolVersion { get; }

        /// <summary>
        /// 
        /// </summary>
        public NetworkProtocol()
        {
            LoaderManager.LoadIfNotLoaded();
        }

        /// <summary>
        /// Happens when parsing a <see cref="NetworkMessage"/> instance
        /// </summary>
        /// <param name="packet">Packet to parse</param>
        /// <returns>Parsed packet to byte array</returns>
        public virtual byte[] OnParseMessage(NetworkMessage packet)
        {
            return DefaultNetworkProtocol.Instance.OnParseMessage(packet);
        }

        /// <summary>
        /// Happens when receiving data from a <see cref="NetTcpClient"/>
        /// </summary>
        /// <param name="client">The connection that received the data</param>
        /// <param name="reader">The data in a <see cref="BasicReader"/> instance</param>
        public virtual void OnReceiveData(NetTcpClient client, BasicReader reader)
        {
            DefaultNetworkProtocol.Instance.OnReceiveData(client, reader);
        }
        /// <summary>
        /// Happens when receiving data from a <see cref="NetUdpClient"/>
        /// </summary>
        /// <param name="client">The connection that received the data</param>
        /// <param name="reader">The data in a <see cref="BasicReader"/> instance</param>
        public virtual void OnReceiveData(NetUdpClient client, BasicReader reader)
        {
            DefaultNetworkProtocol.Instance.OnReceiveData(client, reader);
        }
        /// <summary>
        /// Happens when receiving data from a <see cref="NetTcpConnection"/>
        /// </summary>
        /// <param name="connection">The connection that received the data</param>
        /// <param name="reader">The data in a <see cref="BasicReader"/> instance</param>
        public virtual void OnReceiveData(NetTcpConnection connection, BasicReader reader)
        {
            DefaultNetworkProtocol.Instance.OnReceiveData(connection, reader);
        }
        /// <summary>
        /// Happens when receiving data from a <see cref="NetUdpConnection"/>
        /// </summary>
        /// <param name="connection">The connection that received the data</param>
        /// <param name="reader">The data in a <see cref="BasicReader"/> instance</param>
        public virtual void OnReceiveData(NetUdpConnection connection, BasicReader reader)
        {
            DefaultNetworkProtocol.Instance.OnReceiveData(connection, reader);
        }
    
    }
}
