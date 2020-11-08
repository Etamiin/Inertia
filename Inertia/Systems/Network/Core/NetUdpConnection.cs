using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Network
{
    /// <summary>
    /// Represents a UDP connection that has already sent data to a <see cref="NetUdpServer"/>
    /// </summary>
    public class NetUdpConnection : IDisposable
    {
        #region Public variables

        /// <summary>
        /// Return true if the current instance is disposed
        /// </summary>
        public bool IsDisposed { get; private set; }

        #endregion

        #region Internal variables

        internal IPEndPoint EndPoint;

        #endregion

        #region Private variables

        private NetUdpServer m_server;

        #endregion

        #region Constructors

        internal NetUdpConnection(NetUdpServer server, IPEndPoint endPoint)
        {
            m_server = server;
            EndPoint = endPoint;
        }

        #endregion

        /// <summary>
        /// Send a byte array to the current UDP connection
        /// </summary>
        /// <param name="data">Data to send</param>
        public void Send(byte[] data)
        {
            m_server.SendTo(this, data);
        }
        /// <summary>
        /// Send a <see cref="NetworkMessage"/>instance to the current UDP connection
        /// </summary>
        /// <param name="packet">Packet to send</param>
        public void Send(NetworkMessage packet)
        {
            m_server.SendTo(this, packet);
        }

        /// <summary>
        /// Dispose the current instance
        /// </summary>
        public void Dispose()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(NetUdpConnection));

            m_server = null;
            IsDisposed = true;
        }
    }
}
