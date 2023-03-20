using Inertia.Logging;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Inertia.Network
{
    public abstract class TcpServerEntity : NetworkServerEntity, IDisposable
    {
        public bool IsDisposed { get; private set; }
        public bool IsRunning => _socket != null && _socket.IsBound;
        public int ConnectedCount => _connections.Count;

        private Socket _socket;
        private readonly ConcurrentDictionary<uint, TcpConnectionEntity> _connections;
        private ILogger _logger;

        protected TcpServerEntity(int port) : this(port, Logger.Instance)
        {
        }
        protected TcpServerEntity(string ip, int port) : this(ip, port, Logger.Instance)
        {

        }
        protected TcpServerEntity(int port, ILogger logger) : this(string.Empty, port, logger)
        {
        }
        protected TcpServerEntity(string ip, int port, ILogger logger) : base(ip, port)
        {
            _connections = new ConcurrentDictionary<uint, TcpConnectionEntity>();
            _logger = logger;
        }

        public NetworkConnectionGroup CreateConnectionGroup()
        {
            var group = new NetworkConnectionGroup();

            group.AddConnections(_connections.Values);

            return group;
        }
        public NetworkConnectionGroup CreateConnectionGroup(Predicate<TcpConnectionEntity> predicate)
        {
            var group = new NetworkConnectionGroup();
            var connections = _connections.Values
                .Where((connection) => predicate(connection));

            group.AddConnections(connections);

            return group;
        }
        
        public bool TryGetConnection(uint id, out TcpConnectionEntity connection)
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
                    _socket.Bind(new IPEndPoint(string.IsNullOrEmpty(_ip) ? IPAddress.Any : IPAddress.Parse(_ip), _port));
                    _socket.Listen(1000);

                    Started();
                    _socket.BeginAccept(new AsyncCallback(OnAcceptConnection), _socket);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
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

        protected virtual void OnConnectionConnected(TcpConnectionEntity connection) { }
        protected virtual void OnConnectionDisconnecting(TcpConnectionEntity connection, NetworkDisconnectReason reason) { }

        private void OnAcceptConnection(IAsyncResult iar)
        {
            try
            {
                var socket = ((Socket)iar.AsyncState).EndAccept(iar);
                var connection = new TcpConnectionEntity(socket, (uint)_idProvider.NextInt());

                _connections.TryAdd(connection.Id, connection);

                connection.Disconnecting += ConnectionDisconnected;
                connection.BeginReceiveMessages();

                OnConnectionConnected(connection);
            }
            catch (Exception e)
            {
                if (e is SocketException || e is ObjectDisposedException)
                {
                    return;
                }
            }

            if (IsRunning)
            {
                _socket.BeginAccept(OnAcceptConnection, _socket);
            }
        }

        private void ConnectionDisconnected(TcpConnectionEntity connection, NetworkDisconnectReason reason)
        {
            _connections.TryRemove(connection.Id, out _);
            connection.Disconnecting -= ConnectionDisconnected;

            OnConnectionDisconnecting(connection, reason);
        }
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
