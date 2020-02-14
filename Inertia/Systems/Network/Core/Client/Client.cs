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
    public abstract class Client
    {
        #region Public variables

        public abstract NetworkProtocole Protocole { get; }
        public abstract bool IsConnected { get; }

        #endregion

        #region Protected variables

        protected string NetworkIp { get; private set; }
        protected int NetworkPort { get; private set; }

        #endregion

        #region Constructors

        public Client(string ip, int port)
        {
            NetworkIp = ip;
            NetworkPort = port;
        }

        #endregion

        public abstract void Connect();
        public abstract void Disconnect(NetworkDisconnectReason reason);
        public abstract void Send(byte[] data);

        public void ConnectAsync()
        {
            Task.Factory.StartNew(() => Connect());
        }
        public void DisconnectAsync(NetworkDisconnectReason reason)
        {
            Task.Factory.StartNew(() => Disconnect(reason));
        }

        public void Send<T>(params object[] values) where T : NetworkPacket
        {
            NetworkMessage.Create<T>(values)
                .AddClient(this)
                .SendAndDispose();
        }

        protected void BaseDispose()
        {
            NetworkIp = null;
        }
    }
}
