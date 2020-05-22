using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Network
{
    /// <summary>
    /// Auto serializing network packet
    /// </summary>
    [Serializable]
    public abstract class NetPacket
    {
        #region Public variables

        /// <summary>
        /// Get the id of the current packet
        /// </summary>
        public abstract uint Id { get; }

        #endregion

        /// <summary>
        /// Happens when receiving this packet from a <see cref="NetTcpClient"/>
        /// </summary>
        /// <param name="client">Client that received the packet</param>
        internal protected virtual void OnReceived(NetTcpClient client) { }
        /// <summary>
        /// Happens when receiving this packet from a <see cref="NetUdpClient"/>
        /// </summary>
        /// <param name="client">Client that received the packet</param>
        internal protected virtual void OnReceived(NetUdpClient client) { }
        /// <summary>
        /// Happens when receiving this packet from a <see cref="NetTcpConnection"/>
        /// </summary>
        /// <param name="connection">Connection that received the packet</param>
        internal protected virtual void OnReceived(NetTcpConnection connection) { }
        /// <summary>
        /// Happens when receiving this packet from a <see cref="NetUdpConnection"/>
        /// </summary>
        /// <param name="connection">Connection that received the packet</param>
        internal protected virtual void OnReceived(NetUdpConnection connection) { }
    }
}
