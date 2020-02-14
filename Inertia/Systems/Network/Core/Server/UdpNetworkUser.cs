using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Network
{
    public class UdpNetworkUser : NetworkUser, IDisposable
    {
        #region Public variables

        public bool IsInitialized { get; private set; }

        #endregion

        #region Internal variables

        internal IPEndPoint EndPoint { get; private set; }

        #endregion

        #region Private variables

        private UdpNetworkServer UdpServer;

        #endregion

        #region Constructors

        internal UdpNetworkUser(Server server, IPEndPoint endPoint) : base(server)
        {
            UdpServer = (UdpNetworkServer)server;
            EndPoint = endPoint;
            IsInitialized = true;
        }

        #endregion

        public override void Disconnect(NetworkDisconnectReason reason)
        {
            if (!IsInitialized)
                return;

            Server.ReceiveUserDisconnection(this, reason);
            ToTrash();
        }
        public override void Send(byte[] data)
        {
            if (!IsInitialized)
                return;

            UdpServer.SendTo(EndPoint, data);
        }

        private void ToTrash()
        {
            EndPoint = null;
            UdpServer = null;
            IsInitialized = false;
        }

        public void Dispose()
        {
            if (IsInitialized)
                Disconnect(NetworkDisconnectReason.Manual);
            else
                ToTrash();

            BaseDispose();
        }
    }
}
