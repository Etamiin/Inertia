using System;
using System.Net.Sockets;

namespace Inertia.Network
{
    public sealed class TcpConnectionEntity : NetworkConnectionEntity
    {
        internal BasicAction<NetworkDisconnectReason> Disconnected { get; set; }

        public bool IsConnected => _socket != null && _socket.Connected;

        internal readonly uint Id;

        private byte[] _buffer;
        private Socket _socket;
        private BasicReader _reader;
        private bool _disconnectionNotified;
        
        internal TcpConnectionEntity(Socket socket, uint id)
        {
            Id = id;
            _socket = socket;
            _buffer = new byte[NetworkProtocol.NetworkBufferLength];
            _reader = new BasicReader();
        }

        internal void StartReception()
        {
            if (!IsDisposed)
            {
                _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(OnReceiveData), _socket);
            }
        }

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
        public override void Send(NetworkMessage message)
        {
            Send(NetworkProtocol.GetProtocol().OnParseMessage(message));
        }

        public void Disconnect()
        {
            Disconnect(NetworkDisconnectReason.Manual);
        }
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

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    Disconnect();
                    Disconnected = null;
                }
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
