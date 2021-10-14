using System;
using System.Net;
using System.Net.Sockets;

namespace Inertia.Network
{
    /// <summary>
    ///
    /// </summary>
    public sealed class TcpClientEntity : NetworkClientEntity, IDisposable
    {
        private readonly byte[] _buffer;
        private readonly BasicReader _reader;
        private Socket _socket;

        /// <summary>
        /// Instantiate a new instance of the class <see cref="TcpClientEntity"/>
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public TcpClientEntity(string ip, int port) : base(ip, port)
        {
            _buffer = new byte[NetworkProtocol.NetworkBufferLength];
            _reader = new BasicReader();
        }

        /// <summary>
        /// Returns true if the connection is active otherwise false.
        /// </summary>
        /// <returns></returns>
        public override bool IsConnected()
        {
            return _socket != null && _socket.Connected;
        }
        /// <summary>
        /// Start the connection with the indicated ip and port.
        /// </summary>
        public override void Connect()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(TcpClientEntity));
            }

            if (!IsConnected())
            {
                try
                {
                    _disconnectNotified = false;
                    _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    _socket.Connect(new IPEndPoint(IPAddress.Parse(_targetIp), _targetPort));

                    //throw event before starting to receive data
                    OnConnected();

                    _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, OnReceiveData, _socket);
                }
                catch
                {
                    Disconnect(NetworkDisconnectReason.ConnectionFailed);
                }
            }
        }

        /// <summary>
        /// Terminate the connection with the indicated reason.
        /// </summary>
        /// <param name="reason"></param>
        public override void Disconnect(NetworkDisconnectReason reason)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(TcpClientEntity));
            }

            if (IsConnected())
            {
                _socket?.Shutdown(SocketShutdown.Both);
                _socket?.Disconnect(false);
            }
            if (!_disconnectNotified)
            {
                _reader.Clear();
                _disconnectNotified = true;
                OnDisconnected(reason);
            }
        }
        /// <summary>
        /// Sends the indicated data through the current connection.
        /// </summary>
        /// <param name="data"></param>
        public override void Send(byte[] data)
        {
            if (IsDisposed)
            {
                throw new NotImplementedException(nameof(TcpClientEntity));
            }

            if (!_disconnectNotified)
            {
                _socket?.Send(data);
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
                Disconnect(NetworkDisconnectReason.Manual);
                _reader.Dispose();
                _socket?.Dispose();
            }

            base.Dispose(disposing);
        }

        private void OnReceiveData(IAsyncResult iar)
        {
            try
            {
                int received = ((Socket)iar.AsyncState).EndReceive(iar);
                if (received == 0)
                {
                    throw new SocketException();
                }

                var data = new byte[received];
                Array.Copy(_buffer, data, received);

                NetworkProtocol.GetProtocol().OnReceiveData(this, _reader.Fill(data));
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
            {
                _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, OnReceiveData, _socket);
            }
        }
    }
}
