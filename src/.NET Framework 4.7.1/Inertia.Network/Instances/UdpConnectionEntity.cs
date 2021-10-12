using System.Net;

namespace Inertia.Network
{
    /// <summary>
    ///
    /// </summary>
    public sealed class UdpConnectionEntity : NetworkConnectionEntity
    {
        internal IPEndPoint EndPoint { get; set; }

        private UdpServerEntity _server;

        internal UdpConnectionEntity(UdpServerEntity server, IPEndPoint endPoint)
        {
            _server = server;
            EndPoint = endPoint;
        }

        /// <summary>
        /// Sends the specified data through the current connection.
        /// </summary>
        /// <param name="data"></param>
        public override void Send(byte[] data)
        {
            _server.SendTo(this, data);
        }
        /// <summary>
        /// Sends the specified <see cref="NetworkMessage"/> through the current connection.
        /// </summary>
        /// <param name="message"></param>
        public override void Send(NetworkMessage message)
        {
            _server.SendTo(this, message);
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
                _server = null;
            }
        }
    }
}
