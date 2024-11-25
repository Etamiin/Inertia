using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Net;
using System.Linq;
using Inertia.Logging;

namespace Inertia.Network
{
    public abstract class BaseTcpServer<T, TParameters> : NetworkServerEntity<TParameters>, IDisposable 
        where T : TcpConnectionEntity 
        where TParameters : TcpServerParameters
    {
        public bool IsDisposed { get; private set; }
        public bool IsRunning => _socket?.IsBound == true;
        public int ConnectedCount => _connections.Count;

        private bool _closed;

        private protected Socket? _socket { get; private set; }
        private protected ConcurrentDictionary<uint, T> _connections { get; private set; }

        protected BaseTcpServer(TParameters parameters) : base (parameters)
        {
            _connections = new ConcurrentDictionary<uint, T>();
        }

        public bool TryGetConnection(uint id, out T connection)
        {
            return _connections.TryGetValue(id, out connection);
        }
        public sealed override void Start()
        {
            this.ThrowIfDisposable(IsDisposed);

            if (!IsRunning)
            {
                try
                {
                    _connections.Clear();

                    _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    _socket.Bind(new IPEndPoint(string.IsNullOrWhiteSpace(_parameters.Ip) ? IPAddress.Any : IPAddress.Parse(_parameters.Ip), _parameters.Port));
                    _socket.Listen(_parameters.BacklogQueueSize);
                    _closed = false;

                    OnStarted();
                    _socket.BeginAccept(OnAcceptConnection, _socket);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, GetType(), nameof(Start));
                    Close(NetworkDisconnectReason.ConnectionFailed);
                }
            }
        }
        public sealed override void Close(NetworkDisconnectReason reason)
        {
            this.ThrowIfDisposable(IsDisposed);

            if (_connections?.Any() == true)
            {
                try
                {
                    var entities = _connections.Values.ToArray();
                    foreach (var connection in entities)
                    {
                        connection.Dispose();
                    }
                }
                catch 
                {
                    // No need to catch the exception details here, if the process fails, all connections will be cleaned if the instance is reused.
                }
            }

            if (IsRunning)
            {
                _socket?.Close();
                _socket = null;
            }

            if (!_closed)
            {
                _closed = true;
                OnClosed(reason);                
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        internal virtual void OnAcceptConnection(IAsyncResult iar)
        {
            if (IsRunning)
            {
                _socket?.BeginAccept(OnAcceptConnection, _socket);
            }
        }
        protected virtual void OnConnectionEstablished(T connection) { }
        protected virtual void OnConnectionDisconnecting(T connection, NetworkDisconnectReason reason) { }

        private void Dispose(bool disposing)
        {
            if (IsDisposed) return;

            if (disposing)
            {
                Close();
            }

            IsDisposed = true;
        }
    }
}
