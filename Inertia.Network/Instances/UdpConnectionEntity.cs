using System.Net;

namespace Inertia.Network
{
    /// <summary>
    ///
    /// </summary>
    public class UdpConnectionEntity : NetworkConnectionEntity
    {
        internal IPEndPoint EndPoint { get; set; }

        private UdpServerEntity m_server;

        internal UdpConnectionEntity(UdpServerEntity server, IPEndPoint endPoint)
        {
            m_server = server;
            EndPoint = endPoint;
        }

        /// <summary>
        /// Sends the specified data through the current connection.
        /// </summary>
        /// <param name="data"></param>
        public override void Send(byte[] data)
        {
            m_server.SendTo(this, data);
        }
        /// <summary>
        /// Sends the specified <see cref="NetworkMessage"/> through the current connection.
        /// </summary>
        /// <param name="message"></param>
        public override void Send(NetworkMessage message)
        {
            m_server.SendTo(this, message);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                EndPoint = null;
                m_server = null;
            }
        }
    }
}
