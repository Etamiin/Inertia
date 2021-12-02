<<<<<<< HEAD
﻿using System.Net;

namespace Inertia.Network
{
    public sealed class UdpConnectionEntity : NetworkConnectionEntity
=======
﻿using System;
using System.Net;

namespace Inertia.Network
{
    public sealed class UdpConnectionEntity : NetworkConnectionEntity, IDisposable
>>>>>>> premaster
    {
        internal IPEndPoint EndPoint { get; set; }

        private UdpServerEntity _server;

<<<<<<< HEAD
        internal UdpConnectionEntity(UdpServerEntity server, IPEndPoint endPoint)
=======
        internal UdpConnectionEntity(uint id, UdpServerEntity server, IPEndPoint endPoint) : base(id)
>>>>>>> premaster
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

<<<<<<< HEAD
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
=======
        public void Dispose()
        {
            if (!IsDisposed)
            {
                EndPoint = null;
                _server = null;

                IsDisposed = true;
            }
        }
    }
}
>>>>>>> premaster
