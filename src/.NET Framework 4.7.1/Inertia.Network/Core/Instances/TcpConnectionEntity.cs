using System;
using System.Net.Sockets;

namespace Inertia.Network
{
    /// <summary>
    ///
    /// </summary>
    public sealed class TcpConnectionEntity : NetworkConnectionEntity
    {
        internal event NetworkDisconnectHandler Disconnected;

        /// <summary>
        /// Returns true if the connection is active otherwise false.
        /// </summary>
        /// <returns></returns>
        public bool IsConnected => _socket != null && _socket.Connected;

        private byte[] _buffer;
        private Socket _socket;
        private BasicReader _reader;
        private bool _disconnectionNotified;

        internal TcpConnectionEntity(Socket socket)
        {
            _socket = socket;
            _buffer = new byte[NetworkProtocol.NetworkBufferLength];
            _reader = new BasicReader();
        }

        internal void AllowCommunications()
        {
            _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(OnReceiveData), _socket);
        }

        /// <summary>
        /// Sends the specified data through the current connection.
        /// </summary>
        /// <param name="data"></param>
        public override void Send(byte[] data)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(TcpConnectionEntity));
            }

            if (IsConnected)
            {
                _socket?.Send(data);
            }
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
        public void Disconnect()
        {
            Disconnect(NetworkDisconnectReason.Manual);
        }
        /// <summary>
        /// Terminate the connection with the indicated reason.
        /// </summary>
        /// <param name="reason"></param>
        public void Disconnect(NetworkDisconnectReason reason)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(TcpConnectionEntity));
            }

            if (IsConnected)
            {
                _socket?.Shutdown(SocketShutdown.Both);
                _socket?.Disconnect(false);
                _reader?.Dispose();
            }
            if (!_disconnectionNotified)
            {

                _reader = null;
                _buffer = null;
                _socket = null;
                _disconnectionNotified = true;
                Disconnected?.Invoke(reason);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Disconnect();
                Disconnected = null;
            }

            base.Dispose(disposing);
        }

        private void OnReceiveData(IAsyncResult iar)
        {
            if (IsConnected)
            {
                try
                {
                    var received = ((Socket)iar.AsyncState).EndReceive(iar);
                    if (received == 0)
                    {
                        throw new SocketException();
                    }

                    var data = new byte[received];
                    Array.Copy(_buffer, data, received);

                    NetworkProtocol.GetProtocol().OnReceiveData(this, _reader.Fill(data));
                }
                catch (Exception ex)
                {
                    if (ex is SocketException || ex is ObjectDisposedException)
                    {
                        if (!IsDisposed)
                        {
                            Disconnect(NetworkDisconnectReason.ConnectionLost);
                            return;
                        }
                    }
                }

                if (IsConnected)
                {
                    _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(OnReceiveData), _socket);
                }
            }
        }
    }
}
