using System.Net;

namespace Inertia.Network
{
    public sealed class UdpConnectionEntity : NetworkConnectionEntity
    {
        internal IPEndPoint EndPoint { get; set; }

        private UdpServerEntity _server;

        internal UdpConnectionEntity(UdpServerEntity server, IPEndPoint endPoint)
        {
            _server = server;
            EndPoint = endPoint;
        }

        public override void Send(byte[] data)
        {
            _server.SendTo(this, data);
        }        
        public override void Send(NetworkMessage message)
        {
            _server.SendTo(this, message);
        }

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
