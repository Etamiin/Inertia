using System;
using System.Net.Sockets;

namespace Inertia.Network
{
    /// <summary>
    ///
    /// </summary>
    public sealed class TcpConnectionEntity : NetworkConnectionEntity
    {
        internal event NetworkDisconnectHandler Disconnected = (reason) => { };

        /// <summary>
        /// Returns true if the connection is active otherwise false.
        /// </summary>
        /// <returns></returns>
        public bool IsConnected => m_socket != null && m_socket.Connected;

        private byte[] m_buffer;
        private Socket m_socket;
        private BasicReader m_reader;

        internal TcpConnectionEntity(Socket socket)
        {
            m_socket = socket;
            m_buffer = new byte[NetworkProtocol.NetworkBufferLength];
            m_reader = new BasicReader();
            m_socket.BeginReceive(m_buffer, 0, m_buffer.Length, SocketFlags.None, new AsyncCallback(OnReceiveData), m_socket);
        }

        /// <summary>
        /// Sends the specified data through the current connection.
        /// </summary>
        /// <param name="data"></param>
        public override void Send(byte[] data)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(TcpConnectionEntity));
            if (!IsConnected)
                return;

            try { m_socket.Send(data); } catch { }
        }
        /// <summary>
        /// Sends the specified <see cref="NetworkMessage"/> through the current connection.
        /// </summary>
        /// <param name="message"></param>
        public override void Send(NetworkMessage message)
        {
            Send(NetworkProtocol.GetProtocol().OnParseMessage(message));
        }

        /// <summary>
        /// Terminate the connection with the indicated reason.
        /// </summary>
        /// <param name="reason"></param>
        public void Disconnect(NetworkDisconnectReason reason = NetworkDisconnectReason.Manual)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(TcpConnectionEntity));

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
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Disconnect(NetworkDisconnectReason.Manual);
                Disconnected = null;
            }

            base.Dispose(disposing);
        }

        private void OnReceiveData(IAsyncResult iar)
        {
            if (!IsConnected)
                return;

            try
            {
                var received = ((Socket)iar.AsyncState).EndReceive(iar);
                if (received == 0)
                    throw new SocketException();

                var data = new byte[received];
                Array.Copy(m_buffer, data, received);

                NetworkProtocol.GetProtocol().OnReceiveData(this, m_reader.Fill(data));
            }
            catch (Exception ex)
            {
                if (ex is SocketException || ex is ObjectDisposedException)
                {
                    if (IsDisposed)
                        return;

                    Disconnect(NetworkDisconnectReason.ConnectionLost);
                    return;
                }
            }

            if (IsConnected)
                m_socket.BeginReceive(m_buffer, 0, m_buffer.Length, SocketFlags.None, new AsyncCallback(OnReceiveData), m_socket);
        }
    }
}
