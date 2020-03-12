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
    public class TcpNetworkClient : Client, IDisposable
    {
        #region Events

        public event TcpClientConnectedHandler Connected = (Client) => { };
        public event TcpClientDisconnectedHandler Disconnected = (client, reason) => { };

        #endregion

        #region Public variables

        public override NetworkProtocole Protocole => NetworkProtocole.TransmissionControl;
        public override bool IsConnected
        {
            get
            {
                return Client != null && Client.Connected;
            }
        }

        #endregion

        #region Internal variables

        internal InertiaReader TcpReader { get; private set; }

        #endregion

        #region Private variables

        private byte[] Buffer;
        private Socket Client;

        #endregion

        #region Constructors

        public TcpNetworkClient(string ip, int port) : base(ip, port)
        {
            Buffer = new byte[InertiaConfiguration.NetworkBufferLength];
            TcpReader = new InertiaReader();
        }

        #endregion

        public override void Connect()
        {
            if (IsConnected)
                return;

            try
            {
                Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Client.Connect(new IPEndPoint(IPAddress.Parse(NetworkIp), NetworkPort));
                Client.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, (iar) => OnReceiveData(iar), Client);

                Connected(this);
            }
            catch
            {
                Disconnect(NetworkDisconnectReason.Failed);
            }
        }
        public override void Disconnect(NetworkDisconnectReason reason)
        {
            if (reason == NetworkDisconnectReason.Manual && !IsConnected ||
                reason != NetworkDisconnectReason.Manual && Client == null)
                return;

            try
            {
                Client.Shutdown(SocketShutdown.Both);
                Client.Disconnect(false);
                Client.Close();
                Client.Dispose();
                Client = null;
            }
            catch { }

            Disconnected(this, reason);
        }
        public override void Send(byte[] data)
        {
            if (!IsConnected)
                return;

            var asyncArgs = new SocketAsyncEventArgs();
            asyncArgs.SetBuffer(data, 0, data.Length);

            Client.SendAsync(asyncArgs);
        }

        private void OnReceiveData(IAsyncResult iar)
        {
            try
            {
                int received = ((Socket)iar.AsyncState).EndReceive(iar);
                if (received == 0)
                    throw new SocketException();

                var data = new byte[received];
                Array.Copy(Buffer, data, received);

                InternalNetworkProcessor.OnReceiveClientData(this, data);
                Client.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, (next) => OnReceiveData(next), Client);

            }
            catch (Exception e)
            {
                if (e is SocketException || e is ObjectDisposedException)
                {
                    Disconnect(NetworkDisconnectReason.Lost);
                    return;
                }
                else if (IsConnected)
                    Client.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, (next) => OnReceiveData(next), Client);
            }
        }

        public void Dispose()
        {
            Disconnect(NetworkDisconnectReason.Manual);
            BaseDispose();
            TcpReader?.Dispose();
            TcpReader = null;
            Buffer = null;
        }
    }
}
