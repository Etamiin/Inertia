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
    /// Represent the TCP client
    /// </summary>
    public class NetTcpClient : NetClient, IDisposable
    {
        #region Public variables

        /// <summary>
        /// Return true if the client is connected
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return m_socket != null && m_socket.Connected;
            }
        }

        #endregion

        #region Private variables

        private byte[] m_buffer;
        private Socket m_socket;
        private BasicReader m_reader;

        #endregion

        #region Constructors

        /// <summary>
        /// Instantiate a new instance of the class <see cref="NetTcpClient"/>
        /// </summary>
        /// <param name="ip">Ip to use</param>
        /// <param name="port">Port to use</param>
        public NetTcpClient(string ip, int port) : base(ip, port)
        {
            m_buffer = new byte[DefaultNetworkProtocol.NetworkBufferLength];
            m_reader = new BasicReader();
        }

        #endregion

        /// <summary>
        /// Connect the client
        /// </summary>
        public override void Connect()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(NetTcpClient));

            if (IsConnected)
                return;

            try
            {
                m_disconnected = false;
                m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                m_socket.Connect(new IPEndPoint(IPAddress.Parse(m_targetIp), m_targetPort));
                m_socket.BeginReceive(m_buffer, 0, m_buffer.Length, SocketFlags.None, OnReceiveData, m_socket);
            }
            catch
            {
                Disconnect(NetworkDisconnectReason.ConnectionFailed);
                return;
            }

            OnConnected();
        }
        /// <summary>
        /// Disconnect the client
        /// </summary>
        /// <param name="reason">Reason for disconnection</param>
        public override void Disconnect(NetworkDisconnectReason reason = NetworkDisconnectReason.ManuallyDisconnect)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(NetTcpClient));

            if (!IsConnected && m_disconnected)
                return;

            try
            {
                m_socket.Shutdown(SocketShutdown.Both);
                m_socket.Disconnect(false);
                m_socket.Close();
            }
            catch { }

            m_reader.Clear();
            m_disconnected = true;
            OnDisconnected(reason);
        }
        /// <summary>
        /// Send the specified byte array to the server
        /// </summary>
        /// <param name="data">Byte array to send</param>
        public override void Send(byte[] data)
        {
            if (!IsConnected)
                return;

            m_socket.Send(data);
        }

        /// <summary>
        /// Dispose the current instance
        /// </summary>
        public void Dispose()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(NetTcpClient));

            Disconnect(NetworkDisconnectReason.ManuallyDisconnect);
            m_reader.Dispose();
            m_socket?.Dispose();
            m_buffer = null;
            m_reader = null;
            m_socket = null;

            IsDisposed = true;
        }
        
        private void OnReceiveData(IAsyncResult iar)
        {
            try
            {
                int received = ((Socket)iar.AsyncState).EndReceive(iar);
                if (received == 0)
                    throw new SocketException();

                var data = new byte[received];
                Array.Copy(m_buffer, data, received);

                NetworkProtocol.Protocol.OnReceiveData(this, m_reader.Fill(data));
            }
            catch (Exception e)
            {
                if (e is SocketException || e is ObjectDisposedException)
                {
                    Disconnect(NetworkDisconnectReason.ConnectionLost);
                    return;
                }
            }

            if (IsConnected)
                m_socket.BeginReceive(m_buffer, 0, m_buffer.Length, SocketFlags.None, OnReceiveData, m_socket);
        }
    }
}
