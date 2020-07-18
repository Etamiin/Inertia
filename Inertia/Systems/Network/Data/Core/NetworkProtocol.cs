using System;
using System.Collections.Generic;
using System.Linq;
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
        #region Static

        internal static Dictionary<uint, Type> PacketTypes;
        internal static NetworkProtocol Protocol { get; private set; } = DefaultNetworkProtocol.Instance;
        /// <summary>
        /// Set a custom protocol instance to be used by the system
        /// </summary>
        /// <param name="protocol"><see cref="NetworkProtocol"/> instance to use</param>
        public static void SetProtocol(NetworkProtocol protocol)
        {
            Protocol = protocol;
        }
        /// <summary>
        /// Create an instance of the specified Type if it's an <see cref="NetworkMessage"/>
        /// </summary>
        /// <returns></returns>
        public static NetworkMessage CreateInstance<T>() where T : NetworkMessage
        {
            return CreateInstance(typeof(T));
        }
        /// <summary>
        /// Create an instance of the specified Type if it's an <see cref="NetworkMessage"/>
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
            if (!PacketTypes.ContainsKey(packetId))
                return null;

            return CreateInstance(PacketTypes[packetId]);
        }

        #endregion

        /// <summary>
        /// Happens when parsing a <see cref="NetworkMessage"/> instance
        /// </summary>
        /// <param name="packet">Packet to parse</param>
        /// <returns>Parsed packet to byte array</returns>
        public virtual byte[] OnParsePacket(NetworkMessage packet)
        {
            return DefaultNetworkProtocol.Instance.OnParsePacket(packet);
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
