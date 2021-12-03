using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Inertia.Network
{
    public abstract class TcpServerEntity : NetworkServerEntity, IDisposable
    {
        public bool IsRunning => _socket != null && _socket.IsBound && !_closeNotified;

        private readonly Dictionary<uint, TcpConnectionEntity> _connections;
        private Socket _socket;

        public TcpServerEntity(int port) : this(string.Empty, port)
        {
        }
        public TcpServerEntity(string ip, int port) : base(ip, port)
        {
            _connections = new Dictionary<uint, TcpConnectionEntity>();
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
                throw new ObjectDisposedException(nameof(TcpServerEntity));
            }

            if (IsRunning)
            {
                _socket?.Close();
            }
            if (!_closeNotified)
            {
                TcpConnectionEntity[] entities;
                lock (_connections)
                {
                    entities = _connections.Values.ToArray();
                    _connections.Clear();
                }

                foreach (var connection in entities)
                {
                    connection.Dispose();
                }

                _closeNotified = true;
                OnClosed(reason);
            }
        }

        public virtual void OnClientConnected(TcpConnectionEntity connection) { }
        public virtual void OnClientDisconnected(TcpConnectionEntity connection, NetworkDisconnectReason reason) { }
            
        public void Dispose()
        {
            if (!IsDisposed)
            {
                BeforeDispose();
                Close();

                IsDisposed = true;
            }
        }

        private void OnAcceptConnection(IAsyncResult iar)
        {
            if (IsRunning)
            {
                try
                {
                    var socket = ((Socket)iar.AsyncState).EndAccept(iar);
                    lock (_connections)
                    {
                        var connection = new TcpConnectionEntity(socket, (uint)_idProvider.GetId());

                        connection.Disconnected += (reason) =>
                        {
                            _connections.Remove(connection.Id);
                            OnClientDisconnected(connection, reason);
                        };

                        _connections.Add(connection.Id, connection);

                        connection.StartReception();
                        OnClientConnected(connection);
                    }
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
}
