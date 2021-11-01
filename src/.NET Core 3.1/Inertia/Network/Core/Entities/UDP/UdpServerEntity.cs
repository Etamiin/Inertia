using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Inertia.Network
{
    public class UdpServerEntity : NetworkServerEntity, IDisposable
    {
        private BasicAction<UdpConnectionEntity> ConnectionAdded { get; set; }

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

        /// <summary>
        /// Instantiate a new instance of the class <see cref="UdpServerEntity"/>
        /// </summary>
        public UdpServerEntity() : this(string.Empty, 0)
        {
        }
        /// <summary>
        /// Instantiate a new instance of the class <see cref="UdpServerEntity"/>
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public UdpServerEntity(string ip, int port) : base(ip, port)
        {
            _connections = new Dictionary<IPEndPoint, UdpConnectionEntity>();
        }

        public UdpServerEntity CatchOnStarted(BasicAction callback)
        {
            Started = callback;
            return this;
        }
        public UdpServerEntity CatchOnClosed(BasicAction<NetworkDisconnectReason> callback)
        {
            Closed = callback;
            return this;
        }
        public UdpServerEntity CatchOnConnectionAdded(BasicAction<UdpConnectionEntity> callback)
        {
            ConnectionAdded = callback;
            return this;
        }

        public override void Start()
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

                    Started?.Invoke();
                    _client.BeginReceive(new AsyncCallback(OnReceiveData), _client);
                }
                catch
                {
                    Close(NetworkDisconnectReason.ConnectionFailed);
                }
            }
        }        
        public override void Close(NetworkDisconnectReason reason)
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

                Closed?.Invoke(reason);
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

            SendTo(connection.EndPoint, NetworkProtocol.GetProtocol().OnParseMessage(message));
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    Close();
                    ConnectionAdded = null;
                    _client?.Dispose();
                }
            }

            base.Dispose(disposing);
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

            try { _client.Send(data, data.Length, endPoint); } catch { }
        }
        private void OnReceiveData(IAsyncResult iar)
        {
            try
            {
                IPEndPoint endPoint = null;
                var data = ((UdpClient)iar.AsyncState).EndReceive(iar, ref endPoint);

                if (!_connections.ContainsKey(endPoint))
                {
                    var connection = new UdpConnectionEntity(this, endPoint);
                    _connections.Add(endPoint, connection);

                    ConnectionAdded?.Invoke(connection);
                }

                NetworkProtocol.GetProtocol().OnReceiveData(_connections[endPoint], new BasicReader(data));
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