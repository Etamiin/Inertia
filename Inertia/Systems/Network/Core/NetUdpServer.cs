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
    /// Represent the UDP server
    /// </summary>
    public class NetUdpServer : NetServer, IDisposable
    {
        #region Events

        /// <summary>
        /// Occurs when receiving for the first time data from an udp connection
        /// </summary>
        public event NetworkUdpConnectionAddedHandler ConnectionAdded = (connection) => { };

        #endregion

        #region Public variables

        /// <summary>
        /// Return true if <see cref="Start"/> was called successfully
        /// </summary>
        public bool IsInitialized
        {
            get
            {
                return m_client != null;
            }
        }

        #endregion

        #region Private variables

        private UdpClient m_client;
        private Dictionary<IPEndPoint, NetUdpConnection> m_connections;

        #endregion

        #region Constructors

        /// <summary>
        /// Instantiate a new instance of the class <see cref="NetUdpServer"/>
        /// </summary>
        /// <param name="ip">Ip to use</param>
        /// <param name="port">Port to use</param>
        public NetUdpServer(string ip, int port) : base(ip, port)
        {
        }

        #endregion

        /// <summary>
        /// Start the server
        /// </summary>
        public override void Start()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(NetUdpServer));

            if (IsInitialized)
                return;

            try
            {
                m_closed = false;
                m_connections = new Dictionary<IPEndPoint, NetUdpConnection>();
                m_client = new UdpClient(new IPEndPoint(IPAddress.Parse(m_targetIp), m_targetPort));
                m_client.BeginReceive(new AsyncCallback(OnReceiveData), m_client);

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
                throw new ObjectDisposedException(nameof(NetUdpServer));

            if (!IsInitialized && m_closed)
                return;

            try
            {
                m_client.Close();
            }
            catch { }

            m_connections.Clear();
            m_connections = null;
            m_closed = true;

            OnClosed(reason);
        }
        
        /// <summary>
        /// Send byte array to the specified <see cref="NetUdpConnection"/>
        /// </summary>
        /// <param name="connection">Connection to sent data</param>
        /// <param name="data">Data to send</param>
        public void SendTo(NetUdpConnection connection, byte[] data)
        {
            if (connection.IsDisposed)
                throw new ObjectDisposedException(nameof(NetUdpConnection));

            SendTo(connection.EndPoint, data);
        }
        /// <summary>
        /// Send <see cref="NetworkMessage"/> instance to the specified <see cref="NetUdpConnection"/>
        /// </summary>
        /// <param name="connection">Connection to sent data</param>
        /// <param name="packet">Packet to send</param>
        public void SendTo(NetUdpConnection connection, NetworkMessage packet)
        {
            if (connection.IsDisposed)
                throw new ObjectDisposedException(nameof(NetUdpConnection));

            SendTo(connection.EndPoint, NetworkProtocol.GetProtocol().OnParseMessage(packet));
        }

        /// <summary>
        /// Dispose the current instance
        /// </summary>
        public void Dispose()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(NetUdpServer));

            Close();
            m_client = null;
            IsDisposed = true;
        }

        private void SendTo(IPEndPoint endPoint, byte[] data)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(NetUdpServer));

            if (!IsInitialized)
                return;

            if (data.Length > ushort.MaxValue)
                throw new UserDatagramDataLengthLimitException(data.Length);

            try { m_client.Send(data, data.Length, endPoint); } catch { }
        }
        private void OnReceiveData(IAsyncResult iar)
        {
            try
            {
                IPEndPoint endPoint = null;
                var data = ((UdpClient)iar.AsyncState).EndReceive(iar, ref endPoint);

                if (!m_connections.ContainsKey(endPoint)) {
                    var connection = new NetUdpConnection(this, endPoint);
                    m_connections.Add(endPoint, connection);

                    ConnectionAdded(connection);
                }

                NetworkProtocol.GetProtocol().OnReceiveData(m_connections[endPoint], new BasicReader(data));
            }
            catch (Exception ex) 
            {
                if (ex is SocketException || ex is ObjectDisposedException)
                    return;
            }

            if (IsInitialized)
                m_client.BeginReceive(new AsyncCallback(OnReceiveData), m_client);
        }
    }
}