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

        internal static NetworkProtocol Protocol { get; private set; } = DefaultNetworkProtocol.Instance;
        /// <summary>
        /// Set a custom protocol instance to be used by the system
        /// </summary>
        /// <param name="protocol"><see cref="NetworkProtocol"/> instance to use</param>
        public static void SetProtocol(NetworkProtocol protocol)
        {
            Protocol = protocol;
        }

        #endregion

        /// <summary>
        /// Happens when parsing a <see cref="NetPacket"/> instance
        /// </summary>
        /// <param name="packet">Packet to parse</param>
        /// <returns>Parsed packet to byte array</returns>
        public abstract byte[] OnParsePacket(NetPacket packet);
        /// <summary>
        /// Happens when parsing a <see cref="CustomNetPacket"/> instance
        /// </summary>
        /// <param name="packet">Packet to parse</param>
        /// <returns>Parsed packet to byte array</returns>
        public abstract byte[] OnParsePacket(CustomNetPacket packet);
        /// <summary>
        /// Happens when receiving data from a <see cref="NetTcpClient"/>
        /// </summary>
        /// <param name="client">The connection that received the data</param>
        /// <param name="reader">The data in a <see cref="SimpleReader"/> instance</param>
        public abstract void OnReceiveData(NetTcpClient client, SimpleReader reader);
        /// <summary>
        /// Happens when receiving data from a <see cref="NetUdpClient"/>
        /// </summary>
        /// <param name="client">The connection that received the data</param>
        /// <param name="reader">The data in a <see cref="SimpleReader"/> instance</param>
        public abstract void OnReceiveData(NetUdpClient client, SimpleReader reader);
        /// <summary>
        /// Happens when receiving data from a <see cref="NetTcpConnection"/>
        /// </summary>
        /// <param name="connection">The connection that received the data</param>
        /// <param name="reader">The data in a <see cref="SimpleReader"/> instance</param>
        public abstract void OnReceiveData(NetTcpConnection connection, SimpleReader reader);
        /// <summary>
        /// Happens when receiving data from a <see cref="NetUdpConnection"/>
        /// </summary>
        /// <param name="connection">The connection that received the data</param>
        /// <param name="reader">The data in a <see cref="SimpleReader"/> instance</param>
        public abstract void OnReceiveData(NetUdpConnection connection, SimpleReader reader);
    }
}
