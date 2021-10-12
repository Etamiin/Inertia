using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Inertia.Network
{
    /// <summary>
    ///
    /// </summary>
    public sealed class TcpServerEntity : NetworkServerEntity, IDisposable
    {
        /// <summary>
        /// Occurs when a new client is connected.
        /// </summary>
        public event NetworkTcpClientConnectionCreatedHandler ClientConnected = (connection) => { };
        /// <summary>
        /// Occurs when a client disconnect.
        /// </summary>
        public event NetworkTcpClientConnectionDisconnectedHandler ClientDisconnected = (connection, reason) => { };

        private Socket _socket;
        private List<TcpConnectionEntity> _connections;

        /// <summary>
        /// Instantiate a new instance of the class <see cref="TcpServerEntity"/>
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public TcpServerEntity(string ip, int port) : base(ip, port)
        {
            _connections = new List<TcpConnectionEntity>();
        }

        /// <summary>
        /// Start the server.
        /// </summary>
        public override void Start()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(TcpServerEntity));
            }

            if (!IsRunning())
            {
                try
                {
                    _connections.Clear();
                    _closeNotified = false;
                    _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    _socket.Bind(new IPEndPoint(IPAddress.Parse(_targetIp), _targetPort));
                    _socket.Listen(1000);
                    _socket.BeginAccept(new AsyncCallback(OnAcceptConnection), _socket);

                    OnStarted();
                }
                catch
                {
                    Close(NetworkDisconnectReason.ConnectionFailed);
                }
            }
        }

        /// <summary>
        /// Close the server with the specified reason.
        /// </summary>
        /// <param name="reason"></param>
        public override void Close(NetworkDisconnectReason reason)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(TcpServerEntity));
            }

            if (IsRunning() || !_closeNotified)
            {
                _socket?.Shutdown(SocketShutdown.Both);
                _socket?.Disconnect(false);

                TcpConnectionEntity[] connections;
                lock (_connections)
                {
                    connections = _connections.ToArray();
                }

                foreach (var connection in connections)
                {
                    connection.Dispose();
                }

                _closeNotified = true;
                OnClosed(reason);
            }
        }

        /// <summary>
        /// Returns true if the server is currently running otherwise false.
        /// </summary>
        /// <returns></returns>
        public bool IsRunning()
        {
            return _socket != null && _socket.IsBound;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Close();

                ClientConnected = null;
                ClientDisconnected = null;

                _connections.Clear();
            }

            base.Dispose(disposing);
        }

        private void OnAcceptConnection(IAsyncResult iar)
        {
            if (IsRunning())
            {
                try
                {
                    var socket = ((Socket)iar.AsyncState).EndAccept(iar);
                    var connection = new TcpConnectionEntity(socket);

                    connection.Disconnected += (reason) =>
                    {
                        lock (_connections)
                        {
                            _connections.Remove(connection);
                        }

                        ClientDisconnected?.Invoke(connection, reason);
                    };

                    lock (_connections)
                    {
                        _connections.Add(connection);
                    }

                    ClientConnected?.Invoke(connection);
                }
                catch { }

                if (IsRunning())
                {
                    _socket.BeginAccept(new AsyncCallback(OnAcceptConnection), _socket);
                }
            }            
        }
    }
}
