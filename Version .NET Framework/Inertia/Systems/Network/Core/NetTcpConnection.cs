using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Network
{
    /// <summary>
    /// Represents a TCP connection that is connected to a <see cref="NetTcpServer"/>
    /// </summary>
    public class NetTcpConnection : IDisposable
    {
        #region Events

        internal event NetworkDisconnectHandler Disconnected = (reason) => { };

        #endregion

        #region Public variables

        /// <summary>
        /// Return true if the current instance is disposed
        /// </summary>
        public bool IsDisposed { get; private set; }
        /// <summary>
        /// Return true if the current instance is connected
        /// </summary>
        public bool IsConnected => m_socket != null && m_socket.Connected;

        #endregion

        #region Private variables

        private byte[] m_buffer;
        private Socket m_socket;
        private BasicReader m_reader;

        #endregion

        #region Constructors

        internal NetTcpConnection(Socket socket)
        {
            m_socket = socket;
            m_buffer = new byte[DefaultNetworkProtocol.NetworkBufferLength];
            m_reader = new BasicReader();
            m_socket.BeginReceive(m_buffer, 0, m_buffer.Length, SocketFlags.None, new AsyncCallback(OnReceiveData), m_socket);
        }

        #endregion

        /// <summary>
        /// Disconnect the current instance
        /// </summary>
        /// <param name="reason">Reason for disconnection</param>
        public void Disconnect(NetworkDisconnectReason reason = NetworkDisconnectReason.ManuallyDisconnect)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(NetTcpConnection));

            if (!IsConnected)
                return;

            try
            {
                m_socket.Close();
                m_socket.Dispose();
                m_socket = null;
            }
            catch { }

            m_reader.Dispose();
            m_reader = null;
            m_buffer = null;
            Disconnected(reason);
        }

        /// <summary>
        /// Send a byte array to the current TCP connection
        /// </summary>
        /// <param name="data">Data to send</param>
        public void Send(byte[] data)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(NetTcpConnection));

            if (!IsConnected)
                return;

            try { m_socket.Send(data); } catch { }
        }
        /// <summary>
        /// Send a <see cref="NetworkMessage"/>instance to the current TCP connection
        /// </summary>
        /// <param name="packet">Packet to send</param>
        public void Send(NetworkMessage packet)
        {
            Send(NetworkProtocol.GetProtocol().OnParseMessage(packet));
        }

        /// <summary>
        /// Dispose the current instance
        /// </summary>
        public void Dispose()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(NetTcpConnection));

            Disconnect(NetworkDisconnectReason.ManuallyDisconnect);
            IsDisposed = true;
        }

        private void OnReceiveData(IAsyncResult iar)
        {
            try
            {
                var received = ((Socket)iar.AsyncState).EndReceive(iar);
                if (received == 0)
                    throw new SocketException();

                var data = new byte[received];
                Array.Copy(m_buffer, data, received);

                NetworkProtocol.GetProtocol().OnReceiveData(this, m_reader.Fill(data));
            }
            catch (Exception e)
            {
                if (e is SocketException || e is ObjectDisposedException)
                {
                    if (IsDisposed)
                        return;

                    Disconnect(NetworkDisconnectReason.ConnectionLost);
                    return;
                }
            }

            try
            {
                if (IsConnected)
                    m_socket.BeginReceive(m_buffer, 0, m_buffer.Length, SocketFlags.None, new AsyncCallback(OnReceiveData), m_socket);
            }
            catch { }
        }
    }
}
