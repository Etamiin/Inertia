using Inertia.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace Inertia.Network
{
    public abstract class TcpServerEntity : NetworkServerEntity, IDisposable
    {
        public bool IsRunning => _socket != null && _socket.IsBound && !_closeNotified;
        public int ConnectedCount => _connections.Count;

        private Socket _socket;
        private readonly Dictionary<uint, TcpConnectionEntity> _connections;
        private object _locker;
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
            _locker = new object();
            _connections = new Dictionary<uint, TcpConnectionEntity>();
            _logger = logger;
        }

        public NetworkConnectionGroup CreateConnectionGroup()
        {
            var group = new NetworkConnectionGroup();

            lock (_locker)
            {
                group.AddConnections(_connections.Values);
            }

            return group;
        }
        public NetworkConnectionGroup CreateConnectionGroup(Predicate<TcpConnectionEntity> predicate)
        {
            var group = new NetworkConnectionGroup();

            lock (_locker)
            {
                var targets = new List<TcpConnectionEntity>();
                foreach (var c in _connections.Values)
                {
                    if (predicate(c))
                    {
                        targets.Add(c);
                    }
                }

                group.AddConnections(targets);
            }

            return group;
        }

        internal void ConnectionDisconnected(TcpConnectionEntity connection, NetworkDisconnectReason reason)
        {
            lock (_locker)
            {
                _connections.Remove(connection.Id);
            }

            OnClientDisconnected(connection, reason);
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
                    _closeNotified = false;
                    _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    _socket.Bind(new IPEndPoint(string.IsNullOrEmpty(_targetIp) ? IPAddress.Any : IPAddress.Parse(_targetIp), _targetPort));
                    _socket.Listen(1000);

                    OnStarted();
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

            if (IsRunning)
            {
                _socket?.Close();
            }
            if (!_closeNotified)
            {
                TcpConnectionEntity[] entities;
                lock (_locker)
                {
                    entities = _connections.Values.ToArray();
                }

                foreach (var connection in entities)
                {
                    connection.Dispose();
                }

                _closeNotified = true;
                OnClosed(reason);
            }
        }

        protected virtual void OnClientConnected(TcpConnectionEntity connection) { }
        protected virtual void OnClientDisconnected(TcpConnectionEntity connection, NetworkDisconnectReason reason) { }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                Close();

                IsDisposed = true;
            }
        }

        private void OnAcceptConnection(IAsyncResult iar)
        {
            try
            {
                var socket = ((Socket)iar.AsyncState).EndAccept(iar);
                var connection = new TcpConnectionEntity(this, socket, (uint)_idProvider.NextInt());

                lock (_locker)
                {
                    _connections.Add(connection.Id, connection);
                }

                connection.StartReception();
                OnClientConnected(connection);
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
                _socket.BeginAccept(new AsyncCallback(OnAcceptConnection), _socket);
            }
        }
    }
}
