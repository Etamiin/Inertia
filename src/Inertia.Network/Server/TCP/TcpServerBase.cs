using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Net;

namespace Inertia.Network
{
    public abstract class TcpServerBase<TConnection> : ServerEntity, IDisposable where TConnection : TcpConnectionEntity
    {
        private protected Socket? _socket { get; private set; }
        private protected ConcurrentDictionary<uint, TConnection> _connections { get; private set; }
        private bool _isStarted;

        protected TcpServerBase(string ip, int port) : base (ip, port)
        {
            _connections = new ConcurrentDictionary<uint, TConnection>();
        }

        public bool IsDisposed { get; private set; }
        public bool IsRunning => _isStarted && _socket?.IsBound == true;
        public int ConnectedCount => _connections.Count;

        internal virtual void OnAcceptConnection(IAsyncResult iar)
        {
            if (IsRunning)
            {
                _socket?.BeginAccept(OnAcceptConnection, _socket);
            }
        }

        public sealed override void Start()
        {
            Check.ThrowsIfDisposable(this, IsDisposed);

            if (IsRunning)
            {
                throw new InvalidOperationException("Server is already running.");
            }

            _connections.Clear();

            var ipAddress = string.IsNullOrWhiteSpace(Ip) ? IPAddress.Any : IPAddress.Parse(Ip);

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Bind(new IPEndPoint(ipAddress, Port));
            _socket.Listen(200);
            _isStarted = true;

            OnStarted();
            _socket.BeginAccept(OnAcceptConnection, _socket);
        }
        public sealed override void Stop(NetworkStopReason reason)
        {
            Check.ThrowsIfDisposable(this, IsDisposed);

            if (!_isStarted)
            {
                throw new InvalidOperationException("Server is not running.");
            }

            foreach (var pair in _connections)
            {
                try
                {
                    pair.Value.Dispose();
                }
                catch
                {
                    // no-op
                }
            }

            if (IsRunning)
            {
                _socket?.Close();
                _socket = null;
            }

            _isStarted = false;
            OnStopped(reason);
        }

        public bool TryGetConnection(uint id, out TConnection connection)
        {
            return _connections.TryGetValue(id, out connection);
        }
        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void OnConnectionEstablished(TConnection connection) { }
        protected virtual void OnConnectionDisconnecting(TConnection connection, NetworkStopReason reason) { }

        private void Dispose(bool disposing)
        {
            if (!IsDisposed && disposing)
            {
                Stop();

                IsDisposed = true;
            }
        }
    }
}
