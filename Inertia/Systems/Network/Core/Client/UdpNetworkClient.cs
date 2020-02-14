using Inertia.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Network
{
    public class UdpNetworkClient : Client, IDisposable
    {
        #region Events

        public event UdpClientConnectedHandler Initialized = (client) => { };
        public event UdpClientDisconnectedHandler Disconnected = (client, reason) => { };

        #endregion

        #region Public variables

        public override NetworkProtocole Protocole => NetworkProtocole.UserDatagram;
        public override bool IsConnected
        {
            get
            {
                return UdpClient?.Client == null ? false : UdpClient.Client.Connected;
            }
        }

        #endregion

        #region Private variables

        private UdpClient UdpClient;

        #endregion

        #region Constructors

        public UdpNetworkClient(string ip, int port) : base(ip, port)
        {
        }

        #endregion

        public override void Connect()
        {
            if (IsConnected)
                return;

            try
            {
                UdpClient = new UdpClient();
                UdpClient.Connect(new IPEndPoint(IPAddress.Parse(NetworkIp), NetworkPort));
                UdpClient.BeginReceive(new AsyncCallback(ReceiveFromUserDatagram), UdpClient);

                Initialized(this);
            }
            catch
            {
                Disconnect(NetworkDisconnectReason.Failed);
            }
        }
        public override void Disconnect(NetworkDisconnectReason reason)
        {
            if (reason == NetworkDisconnectReason.Manual && !IsConnected ||
                reason != NetworkDisconnectReason.Manual && UdpClient == null)
                return;

            try
            {
                UdpClient.Close();
            }
            catch { }

            Disconnected(this, reason);
        }
        public override void Send(byte[] data)
        {
            if (!IsConnected)
                return;

            UdpClient.SendAsync(data, data.Length);
        }

        private void ReceiveFromUserDatagram(IAsyncResult iar)
        {
            if (!IsConnected)
                return;

            try
            {
                IPEndPoint endPoint = null;
                var data = ((UdpClient)iar.AsyncState).EndReceive(iar, ref endPoint);

                InternalNetworkProcessor.OnReceiveClientData(this, data);
                UdpClient.BeginReceive(new AsyncCallback(ReceiveFromUserDatagram), UdpClient);
            }
            catch (Exception e)
            {
                if (e is SocketException || e is ObjectDisposedException)
                {
                    Disconnect(NetworkDisconnectReason.Lost);
                    return;
                }
                else if (IsConnected)
                    UdpClient.BeginReceive(new AsyncCallback(ReceiveFromUserDatagram), UdpClient);
            }
        }

        public void Dispose()
        {
            Disconnect(NetworkDisconnectReason.Manual);
            BaseDispose();
            UdpClient?.Dispose();
            UdpClient = null;
        }
    }
}
