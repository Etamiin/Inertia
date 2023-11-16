using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Net;
using System.Linq;
using System.Collections.Generic;

namespace Inertia.Network
{
    public abstract class BaseTcpServer<T, TParameters> : NetworkServerEntity<TParameters>, IDisposable where T : TcpConnectionEntity where TParameters : TcpServerParameters
    {
        public bool IsDisposed { get; private set; }
        public bool IsRunning => _socket != null && _socket.IsBound;
        public int ConnectedCount => _connections.Count;

        private protected Socket? _socket { get; private set; }

        private protected readonly ConcurrentDictionary<uint, T> _connections;

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

                    OnStarted();
                    _socket.BeginAccept(OnAcceptConnection, _socket);
                }
                catch (Exception ex)
                {
                    Logger.Error($"Starting server failed: {ex}");
                    Close(NetworkDisconnectReason.ConnectionFailed);
                }
            }
        }
        public sealed override void Close(NetworkDisconnectReason reason)
        {
            this.ThrowIfDisposable(IsDisposed);

            if (_connections != null && _connections.Count > 0)
            {
                var entities = _connections.Values.ToArray();
                foreach (var connection in entities)
                {
                    connection.Dispose();
                }
            }

            if (IsRunning)
            {
                _socket?.Close();
                _socket = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        internal abstract void OnAcceptConnection(IAsyncResult iar);
        protected virtual void OnConnectionConnected(T connection) { }
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
