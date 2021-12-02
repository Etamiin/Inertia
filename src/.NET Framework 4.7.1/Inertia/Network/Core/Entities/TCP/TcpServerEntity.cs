using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Inertia.Network
{
<<<<<<< HEAD
    public sealed class TcpServerEntity : NetworkServerEntity, IDisposable
    {
        private static uint _ids;

        private BasicAction<TcpConnectionEntity> ClientConnected { get; set; }
        private BasicAction<TcpConnectionEntity, NetworkDisconnectReason> ClientDisconnected { get; set; }

=======
    public abstract class TcpServerEntity : NetworkServerEntity, IDisposable
    {
>>>>>>> premaster
        public bool IsRunning => _socket != null && _socket.IsBound && !_closeNotified;

        private readonly Dictionary<uint, TcpConnectionEntity> _connections;
        private Socket _socket;

<<<<<<< HEAD
        /// <summary>
        /// Instantiate a new instance of the class <see cref="TcpServerEntity"/>
        /// </summary>
        public TcpServerEntity() : this(string.Empty, 0)
        {
        }
        /// <summary>
        /// Instantiate a new instance of the class <see cref="TcpServerEntity"/>
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
=======
        public TcpServerEntity(int port) : this(string.Empty, port)
        {
        }
>>>>>>> premaster
        public TcpServerEntity(string ip, int port) : base(ip, port)
        {
            _connections = new Dictionary<uint, TcpConnectionEntity>();
        }

<<<<<<< HEAD
        public TcpServerEntity CatchOnStarted(BasicAction callback)
        {
            Started = callback;
            return this;
        }
        public TcpServerEntity CatchOnClosed(BasicAction<NetworkDisconnectReason> callback)
        {
            Closed = callback;
            return this;
        }
        public TcpServerEntity CatchOnClientConnected(BasicAction<TcpConnectionEntity> callback)
        {
            ClientConnected = callback;
            return this;
        }
        public TcpServerEntity CatchOnClientDisconnected(BasicAction<TcpConnectionEntity, NetworkDisconnectReason> callback)
        {
            ClientDisconnected = callback;
            return this;
        }

        public override void Start()
=======
        public sealed override void Start()
>>>>>>> premaster
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
<<<<<<< HEAD
                    if (string.IsNullOrEmpty(_targetIp) || _targetPort == 0)
                    {
                        _socket.Bind(new IPEndPoint(IPAddress.Any, 0));
                    }
                    else
                    {
                        _socket.Bind(new IPEndPoint(IPAddress.Parse(_targetIp), _targetPort));
                    }

                    _socket.Listen(1000);
                    Started?.Invoke();
=======
                    _socket.Bind(new IPEndPoint(string.IsNullOrEmpty(_targetIp) ? IPAddress.Any : IPAddress.Parse(_targetIp), _targetPort));

                    _socket.Listen(1000);

                    OnStarted();
>>>>>>> premaster
                    _socket.BeginAccept(new AsyncCallback(OnAcceptConnection), _socket);
                }
                catch
                {
                    Close(NetworkDisconnectReason.ConnectionFailed);
                }
            }
        }
<<<<<<< HEAD
        public override void Close(NetworkDisconnectReason reason)
=======
        public sealed override void Close(NetworkDisconnectReason reason)
>>>>>>> premaster
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
<<<<<<< HEAD
                Closed?.Invoke(reason);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    Close();

                    ClientConnected = null;
                    ClientDisconnected = null;

                    _connections.Clear();
                }
            }

            base.Dispose(disposing);
=======
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
>>>>>>> premaster
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
<<<<<<< HEAD
                        var connection = new TcpConnectionEntity(socket, _ids++);

                        connection.Disconnected = (reason) =>
                        {
                            _connections.Remove(connection.Id);
                            ClientDisconnected?.Invoke(connection, reason);
                        };

                        _connections.Add(connection.Id, connection);
                        ClientConnected?.Invoke(connection);

                        connection.StartReception();
=======
                        var connection = new TcpConnectionEntity(socket, (uint)_idProvider.GetId());

                        connection.Disconnected += (reason) =>
                        {
                            _connections.Remove(connection.Id);
                            OnClientDisconnected(connection, reason);
                        };

                        _connections.Add(connection.Id, connection);

                        connection.StartReception();
                        OnClientConnected(connection);
>>>>>>> premaster
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
