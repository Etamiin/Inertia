using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Inertia.Network
{
    /// <summary>
    ///
    /// </summary>
    public class UdpServerEntity : NetworkServerEntity, IDisposable
    {
        /// <summary>
        /// Occurs when receiving for the first time data from an udp connection.
        /// </summary>
        public event NetworkUdpConnectionAddedHandler ConnectionAdded = (connection) => { };

        /// <summary>
        /// Returns true if <see cref="Start"/> was called successfully.
        /// </summary>
        public bool IsInitialized
        {
            get
            {
                return m_client != null;
            }
        }

        private UdpClient m_client;
        private Dictionary<IPEndPoint, UdpConnectionEntity> m_connections;

        /// <summary>
        /// Instantiate a new instance of the class <see cref="UdpServerEntity"/>
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public UdpServerEntity(string ip, int port) : base(ip, port)
        {
        }

        /// <summary>
        /// Start the server.
        /// </summary>
        public override void Start()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(UdpServerEntity));
            if (IsInitialized)
                return;

            try
            {
                m_closed = false;
                m_connections = new Dictionary<IPEndPoint, UdpConnectionEntity>();
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
        /// Close the server with the specified reason.
        /// </summary>
        /// <param name="reason"></param>
        public override void Close(NetworkDisconnectReason reason = NetworkDisconnectReason.Manual)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(UdpServerEntity));

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
        /// Sends the specified data to the specified connection.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="data"></param>
        public void SendTo(UdpConnectionEntity connection, byte[] data)
        {
            if (connection.IsDisposed)
                throw new ObjectDisposedException(nameof(UdpConnectionEntity));

            SendTo(connection.EndPoint, data);
        }
        /// <summary>
        /// Sends the specified <see cref="NetworkMessage"/> to the specified connection.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="message"></param>
        public void SendTo(UdpConnectionEntity connection, NetworkMessage message)
        {
            if (connection.IsDisposed)
                throw new ObjectDisposedException(nameof(UdpConnectionEntity));

            SendTo(connection.EndPoint, NetworkProtocol.GetProtocol().OnParseMessage(message));
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
                ConnectionAdded = null;
                m_client?.Dispose();
            }

            base.Dispose(disposing);
        }

        private void SendTo(IPEndPoint endPoint, byte[] data)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(UdpServerEntity));
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

                if (!m_connections.ContainsKey(endPoint))
                {
                    var connection = new UdpConnectionEntity(this, endPoint);
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