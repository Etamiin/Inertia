using System;
using System.Net;
using System.Net.Sockets;

namespace Inertia.Network
{
    /// <summary>
    ///
    /// </summary>
    public class TcpClientEntity : NetworkClientEntity, IDisposable
    {
        private byte[] m_buffer;
        private Socket m_socket;
        private BasicReader m_reader;

        /// <summary>
        /// Instantiate a new instance of the class <see cref="TcpClientEntity"/>
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public TcpClientEntity(string ip, int port) : base(ip, port)
        {
            m_buffer = new byte[NetworkProtocol.NetworkBufferLength];
            m_reader = new BasicReader();
        }

        /// <summary>
        /// Returns true if the connection is active otherwise false.
        /// </summary>
        /// <returns></returns>
        public override bool IsConnected()
        {
            return m_socket != null && m_socket.Connected;
        }
        /// <summary>
        /// Start the connection with the indicated ip and port.
        /// </summary>
        public override void Connect()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(TcpClientEntity));
            if (IsConnected())
                return;

            try
            {
                m_disconnected = false;
                m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                m_socket.Connect(new IPEndPoint(IPAddress.Parse(m_targetIp), m_targetPort));
                m_socket.BeginReceive(m_buffer, 0, m_buffer.Length, SocketFlags.None, OnReceiveData, m_socket);

                OnConnected();
            }
            catch
            {
                Disconnect(NetworkDisconnectReason.ConnectionFailed);
            }
        }
        /// <summary>
        /// Terminate the connection with the indicated reason.
        /// </summary>
        /// <param name="reason"></param>
        public override void Disconnect(NetworkDisconnectReason reason = NetworkDisconnectReason.Manual)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(TcpClientEntity));

            if (!IsConnected() && m_disconnected)
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
        /// Sends the indicated data through the current connection.
        /// </summary>
        /// <param name="data"></param>
        public override void Send(byte[] data)
        {
            if (IsDisposed)
                throw new NotImplementedException(nameof(TcpClientEntity));
            if (m_disconnected)
                return;

            try { m_socket.Send(data); } catch { }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Disconnect(NetworkDisconnectReason.Manual);
                m_reader.Dispose();
                m_socket?.Dispose();
            }

            base.Dispose(disposing);
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

                NetworkProtocol.GetProtocol().OnReceiveData(this, m_reader.Fill(data));
            }
            catch (Exception e)
            {
                if (e is SocketException || e is ObjectDisposedException)
                {
                    Disconnect(NetworkDisconnectReason.ConnectionLost);
                    return;
                }
            }

            if (IsConnected())
                m_socket.BeginReceive(m_buffer, 0, m_buffer.Length, SocketFlags.None, OnReceiveData, m_socket);
        }
    }
}
