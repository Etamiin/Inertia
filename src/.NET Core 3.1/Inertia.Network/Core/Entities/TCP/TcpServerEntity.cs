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

        private Socket m_socket;
        private List<TcpConnectionEntity> m_connections;

        /// <summary>
        /// Instantiate a new instance of the class <see cref="TcpServerEntity"/>
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public TcpServerEntity(string ip, int port) : base(ip, port)
        {
            m_connections = new List<TcpConnectionEntity>();
        }

        /// <summary>
        /// Start the server.
        /// </summary>
        public override void Start()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(TcpServerEntity));
            if (IsRunning())
                return;

            try
            {
                m_connections.Clear();
                m_closed = false;
                m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                m_socket.Bind(new IPEndPoint(IPAddress.Parse(m_targetIp), m_targetPort));
                m_socket.Listen(1000);
                m_socket.BeginAccept(new AsyncCallback(OnAcceptConnection), m_socket);

                OnStarted();
            }
            catch
            {
                Close(NetworkDisconnectReason.ConnectionFailed);
            }
        }
        /// <summary>
        /// Close the server with the specified reason.
        /// </summary>
        /// <param name="reason"></param>
        public override void Close(NetworkDisconnectReason reason = NetworkDisconnectReason.Manual)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(TcpServerEntity));
            if (!IsRunning() && m_closed)
                return;

            try
            {
                m_socket.Close();
                m_socket.Dispose();
                m_socket = null;
            }
            catch { }

            TcpConnectionEntity[] connections;
            lock (m_connections)
                connections = m_connections.ToArray();

            foreach (var connection in connections)
                connection.Dispose();

            m_closed = true;
            OnClosed(reason);
        }

        /// <summary>
        /// Returns true if the server is currently running otherwise false.
        /// </summary>
        /// <returns></returns>
        public bool IsRunning()
        {
            return m_socket != null && m_socket.IsBound;
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

                m_connections.Clear();
            }

            base.Dispose(disposing);
        }

        private void OnAcceptConnection(IAsyncResult iar)
        {
            if (!IsRunning())
                return;

            try
            {
                var socket = ((Socket)iar.AsyncState).EndAccept(iar);
                var connection = new TcpConnectionEntity(socket);

                connection.Disconnected += (reason) =>
                {
                    lock (m_connections)
                        m_connections.Remove(connection);

                    ClientDisconnected(connection, reason);
                };

                lock (m_connections)
                    m_connections.Add(connection);

                ClientConnected(connection);
            }
            catch { }

            if (IsRunning())
                m_socket.BeginAccept(new AsyncCallback(OnAcceptConnection), m_socket);
        }
    }
}
