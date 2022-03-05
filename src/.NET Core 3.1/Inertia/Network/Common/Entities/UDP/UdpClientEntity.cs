using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Inertia.Network
{
    public abstract class UdpClientEntity : NetworkClientEntity, IDisposable
    {
        public override bool IsConnected => (_client?.Client) != null && _client.Client.Connected;
        private UdpClient _client;
        private BasicReader _reader;

        protected UdpClientEntity(string ip, int port) : base(ip, port)
        {
        }
        
        public sealed override void Connect()
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
                    _reader = new BasicReader();
                    _client = new UdpClient();
                    _client.Connect(new IPEndPoint(IPAddress.Parse(_targetIp), _targetPort));

                    OnConnected();
                    _client.BeginReceive(new AsyncCallback(OnReceiveData), _client);
                }
                catch
                {
                    Disconnect(NetworkDisconnectReason.ConnectionFailed);
                }
            }
        }        
        public sealed override void Disconnect(NetworkDisconnectReason reason)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(UdpClientEntity));
            }

            if (IsConnected)
            {
                _reader?.Dispose();
                _client?.Close();
            }

            if (!_disconnectNotified)
            {
                _disconnectNotified = true;
                OnDisconnected(reason);
            }
        }
        public sealed override void Send(byte[] data)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(UdpClientEntity));
            }
            if (data.Length > ushort.MaxValue)
            {
                throw new UserDatagramDataLengthLimitException();
            }

            try
            {
                _client.Send(data, data.Length);
            }
            catch { }
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                BeforeDispose();
                Disconnect(NetworkDisconnectReason.Manual);
                _client?.Dispose();

                IsDisposed = true;
            }
        }

        private void OnReceiveData(IAsyncResult iar)
        {
            try
            {
                IPEndPoint endPoint = null;
                var data = ((UdpClient)iar.AsyncState).EndReceive(iar, ref endPoint);

                NetworkProtocol.ProcessParsing(this, _reader.Fill(data));
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
