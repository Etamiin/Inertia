using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Inertia.Internal;

namespace Inertia.Network
{
    public abstract class Server
    {
        #region Events

        public event ServerStartHandler Opened = (server) => { };
        public event ServerStopHandler Closed = (server, reason) => { };

        #endregion

        #region Public variables

        public abstract NetworkProtocole Protocole { get; }
        public abstract bool IsConnected { get; }
        public abstract int UserCount { get; }

        #endregion

        #region Protected variables

        protected string NetworkIp;
        protected int NetworkPort;

        #endregion

        #region Constructors

        public Server(string ip, int port)
        {
            NetworkIp = ip;
            NetworkPort = port;
        }

        #endregion

        public abstract void Open();
        public abstract void Close(NetworkDisconnectReason reason);
        internal abstract void ReceiveUserDisconnection(NetworkUser user, NetworkDisconnectReason reason);

        public void OpenAsync()
        {
            Task.Factory.StartNew(() => Open());
        }
        public void CloseAsync(NetworkDisconnectReason reason)
        {
            Task.Factory.StartNew(() => Close(reason));
        }

        protected void BaseDispose()
        {
            Opened = null;
            Closed = null;
        }
        protected void OnOpen()
        {
            Opened(this);
        }
        protected void OnClose(NetworkDisconnectReason reason)
        {
            Closed(this, reason);
        }
    }
}
