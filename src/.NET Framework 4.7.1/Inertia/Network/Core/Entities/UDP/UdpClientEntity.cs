using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Inertia.Network
{
    public class UdpClientEntity : NetworkClientEntity, IDisposable
    {
        public override bool IsConnected => (_client?.Client) != null && _client.Client.Connected;
        private UdpClient _client;

        /// <summary>
        /// Instantiate a new instance of the class <see cref="UdpClientEntity"/>
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public UdpClientEntity(string ip, int port) : base(ip, port)
        {
        }
        
        public UdpClientEntity CatchOnConnected(BasicAction callback)
        {
            Connected = callback;
            return this;
        }
        public UdpClientEntity CatchOnDisconnected(BasicAction<NetworkDisconnectReason> callback)
        {
            Disconnected = callback;
            return this;
        }

        public override void Connect()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(UdpClientEntity));
            }

            if (!IsConnected)
            {
                try
                {
                    _disconnectNotified = false;
                    _client = new UdpClient();
                    _client.Connect(new IPEndPoint(IPAddress.Parse(_targetIp), _targetPort));

                    Connected?.Invoke();
                    _client.BeginReceive(new AsyncCallback(OnReceiveData), _client);
                }
                catch
                {
                    Disconnect(NetworkDisconnectReason.ConnectionFailed);
                }
            }
        }        
        public override void Disconnect(NetworkDisconnectReason reason)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(UdpClientEntity));
            }

            if (IsConnected)
            {
                _client?.Close();
            }
            if (!_disconnectNotified)
            {
                _disconnectNotified = true;
                Disconnected.Invoke(reason);
            }
        }
        
        public override void Send(byte[] data)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(UdpClientEntity));
            }

            if (data.Length > ushort.MaxValue)
            {
                throw new UserDatagramDataLengthLimitException();
            }

            try { _client.SendAsync(data, data.Length); } catch { }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Disconnect(NetworkDisconnectReason.Manual);
                _client?.Dispose();
            }

            base.Dispose(disposing);
        }

        private void OnReceiveData(IAsyncResult iar)
        {
            if (IsConnected)
            {
                try
                {
                    IPEndPoint endPoint = null;
                    var data = ((UdpClient)iar.AsyncState).EndReceive(iar, ref endPoint);

                    NetworkProtocol.GetProtocol().OnReceiveData(this, new BasicReader(data));
                }
                catch (Exception e)
                {
                    if (e is SocketException || e is ObjectDisposedException)
                    {
                        Disconnect(NetworkDisconnectReason.ConnectionLost);
                        return;
                    }
                }

                if (IsConnected)
                {
                    _client.BeginReceive(new AsyncCallback(OnReceiveData), _client);
                }
            }
        }
    }
}
