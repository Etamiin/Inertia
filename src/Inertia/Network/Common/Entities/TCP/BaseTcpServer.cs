using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Net;
using System.Linq;

namespace Inertia.Network
{
    public abstract class BaseTcpServer<T, TParameters> : NetworkServerEntity<TParameters>, IDisposable where T : TcpConnectionEntity where TParameters : ServerParameters
    {
        public bool IsDisposed { get; private set; }
        public bool IsRunning => _socket != null && _socket.IsBound;
        public int ConnectedCount => _connections.Count;

        private protected Socket? _socket { get; private set; }
        private protected readonly ConcurrentDictionary<uint, T> _connections;

        protected BaseTcpServer(TParameters parameters) : base(parameters)
        {
            _connections = new ConcurrentDictionary<uint, T>();
        }

        public NetworkConnectionGroup CreateConnectionGroup()
        {
            var group = new NetworkConnectionGroup();

            group.AddConnections(_connections.Values);

            return group;
        }
        public NetworkConnectionGroup CreateConnectionGroup(Predicate<T> predicate)
        {
            var group = new NetworkConnectionGroup();
            var connections = _connections.Values
                .Where((connection) => predicate(connection));

            group.AddConnections(connections);

            return group;
        }

        public bool TryGetConnection(uint id, out T connection)
        {
            return _connections.TryGetValue(id, out connection);
        }

        public sealed override void Start()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(TcpServerEntity));
            }

            if (!IsRunning)
            {
                try
                {
                    _connections.Clear();

                    _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    _socket.Bind(new IPEndPoint(string.IsNullOrWhiteSpace(Parameters.Ip) ? IPAddress.Any : IPAddress.Parse(Parameters.Ip), Parameters.Port));
                    _socket.Listen(1000);

                    OnStarted();
                    _socket.BeginAccept(OnAcceptConnection, _socket);
                }
                catch (Exception ex)
                {
                    Logger?.Error(ex);
                    Close(NetworkDisconnectReason.ConnectionFailed);
                }
            }
        }
        public sealed override void Close(NetworkDisconnectReason reason)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(TcpServerEntity));
            }

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

        internal protected abstract void OnAcceptConnection(IAsyncResult iar);

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
