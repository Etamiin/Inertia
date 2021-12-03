using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Inertia.Network
{
    public class UdpServerEntity : NetworkServerEntity, IDisposable
    {
        /// <summary>
        /// Returns true if <see cref="Start"/> was called successfully.
        /// </summary>
        public bool IsInitialized
        {
            get
            {
                return _client != null;
            }
        }

        private UdpClient _client;
        private Dictionary<IPEndPoint, UdpConnectionEntity> _connections;

        public UdpServerEntity(int port) : this(string.Empty, port)
        {
        }
        public UdpServerEntity(string ip, int port) : base(ip, port)
        {
            _connections = new Dictionary<IPEndPoint, UdpConnectionEntity>();
        }

        public sealed override void Start()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(UdpServerEntity));
            }

            if (!IsInitialized)
            {
                try
                {
                    _closeNotified = false;
                    _connections.Clear();

                    if (string.IsNullOrEmpty(_targetIp) || _targetPort == 0)
                    {
                        _client = new UdpClient(new IPEndPoint(IPAddress.Any, _targetPort));
                    }
                    else
                    {
                        _client = new UdpClient(new IPEndPoint(IPAddress.Parse(_targetIp), _targetPort));
                    }

                    OnStarted();
                    _client.BeginReceive(new AsyncCallback(OnReceiveData), _client);
                }
                catch
                {
                    Close(NetworkDisconnectReason.ConnectionFailed);
                }
            }
        }        
        public sealed override void Close(NetworkDisconnectReason reason)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(UdpServerEntity));
            }

            if (IsInitialized)
            {
                _client?.Close();
                _client = null;
            }
            if (!_closeNotified)
            {
                _connections.Clear();
                _closeNotified = true;

                OnClosed(reason);
            }
        }

        public void SendTo(UdpConnectionEntity connection, byte[] data)
        {
            if (connection.IsDisposed)
            {
                throw new ObjectDisposedException(nameof(UdpConnectionEntity));
            }

            SendTo(connection.EndPoint, data);
        }        
        public void SendTo(UdpConnectionEntity connection, NetworkMessage message)
        {
            if (connection.IsDisposed)
            {
                throw new ObjectDisposedException(nameof(UdpConnectionEntity));
            }

            SendTo(connection.EndPoint, NetworkProtocol.GetCurrentProtocol().OnSerializeMessage(message));
        }

        public virtual void OnConnectionAdded(UdpConnectionEntity connection) { }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                Close();
                _client?.Dispose();

                IsDisposed = true;
            }
        }

        private void SendTo(IPEndPoint endPoint, byte[] data)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(UdpServerEntity));
            }

            if (data.Length > ushort.MaxValue)
            {
                throw new UserDatagramDataLengthLimitException();
            }

            _client.Send(data, data.Length, endPoint);
        }
        private void OnReceiveData(IAsyncResult iar)
        {
            try
            {
                IPEndPoint endPoint = null;
                var data = ((UdpClient)iar.AsyncState).EndReceive(iar, ref endPoint);

                if (!_connections.ContainsKey(endPoint))
                {
                    var connection = new UdpConnectionEntity((uint)_idProvider.GetId(), this, endPoint);
                    _connections.Add(endPoint, connection);

                    OnConnectionAdded(connection);
                }

                NetworkProtocol.ProcessParsing(_connections[endPoint], new BasicReader(data));
            }
            catch (Exception ex)
            {
                if (ex is SocketException || ex is ObjectDisposedException)
                {
                    return;
                }
            }

            if (IsInitialized)
            {
                _client.BeginReceive(new AsyncCallback(OnReceiveData), _client);
            }
        }
    }
}