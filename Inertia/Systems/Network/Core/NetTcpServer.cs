using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Network
{
    /// <summary>
    /// Represent the TCP server
    /// </summary>
    public class NetTcpServer : NetServer, IDisposable
    {
        #region Events
        
        /// <summary>
        /// Occurs when a new client is connected
        /// </summary>
        public event NetworkTcpClientConnectionCreatedHandler ClientConnected = (connection) => { };
        /// <summary>
        /// Occurs when a client disconnect
        /// </summary>
        public event NetworkTcpClientConnectionDisconnectedHandler ClientDisconnected = (connection, reason) => { };

        #endregion

        #region Public variables

        /// <summary>
        /// Return true if the server is started
        /// </summary>
        public bool IsRunning
        {
            get
            {
                return m_socket != null && m_socket.IsBound;
            }
        }

        #endregion

        #region Private variables

        private Socket m_socket;
        private List<NetTcpConnection> m_connections;

        #endregion

        #region Constructors

        /// <summary>
        /// Instantiate a new instance of the class <see cref="NetTcpServer"/>
        /// </summary>
        /// <param name="ip">Ip to use</param>
        /// <param name="port">Port to use</param>
        public NetTcpServer(string ip, int port) : base(ip, port)
        {
            m_connections = new List<NetTcpConnection>();
        }

        #endregion

        /// <summary>
        /// Start the server
        /// </summary>
        public override void Start()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(NetTcpServer));

            if (IsRunning)
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
        /// Close the server
        /// </summary>
        /// <param name="reason">Reason for closing the server</param>
        public override void Close(NetworkDisconnectReason reason = NetworkDisconnectReason.ManuallyDisconnect)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(NetTcpServer));

            if (!IsRunning && m_closed)
                return;

            try
            {
                m_socket.Close();
                m_socket.Dispose();
                m_socket = null;
            }
            catch { }

            NetTcpConnection[] connections;
            lock (m_connections)
                connections = m_connections.ToArray();

            foreach (var connection in connections)
                connection.Dispose();

            m_closed = true;
            OnClosed(reason);
        }

        /// <summary>
        /// Dispose the current instance
        /// </summary>
        public void Dispose()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(NetTcpServer));

            m_connections = null;

            IsDisposed = true;
        }

        private void OnAcceptConnection(IAsyncResult iar)
        {
            try
            {
                var socket = ((Socket)iar.AsyncState).EndAccept(iar);
                var connection = new NetTcpConnection(socket);

                connection.Disconnected += (reason) =>
                {
                    m_connections.Remove(connection);
                    ClientDisconnected(connection, reason);
                };

                m_connections.Add(connection);
                ClientConnected(connection);
            }
            catch { }

            try
            {
                if (IsRunning)
                    m_socket.BeginAccept(new AsyncCallback(OnAcceptConnection), m_socket);
            }
            catch { }
        }
    }
}
