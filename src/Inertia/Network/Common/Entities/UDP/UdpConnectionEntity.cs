using System;
using System.Net;

namespace Inertia.Network
{
    [Obsolete]
    public sealed class UdpConnectionEntity : NetworkConnectionEntity, IDisposable
    {
        internal IPEndPoint EndPoint { get; set; }

        private UdpServerEntity _server;

        internal UdpConnectionEntity(uint id, UdpServerEntity server, IPEndPoint endPoint, NetworkProtocol protocol) : base(id, protocol)
        {
            _server = server;
            EndPoint = endPoint;
        }

        public override void Send(byte[] data)
        {
            _server.SendTo(this, data);
        }

        public override bool Disconnect(NetworkDisconnectReason reason)
        {
            return true;
        }

        public void Dispose()
        {
            //if (!IsDisposed)
            //{
            //    EndPoint = null;
            //    _server = null;

            //    IsDisposed = true;
            //}
        }
    }
}