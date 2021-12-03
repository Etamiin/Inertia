using System;
using System.Net.Sockets;

namespace Inertia.Network
{
    public sealed class TcpConnectionEntity : NetworkConnectionEntity, IDisposable
    {
        internal event BasicAction<NetworkDisconnectReason> Disconnected;

        public bool IsConnected => _socket != null && _socket.Connected;

        private byte[] _buffer;
        private Socket _socket;
        private BasicReader _reader;
        private bool _disconnectionNotified;
        
        internal TcpConnectionEntity(Socket socket, uint id) : base(id)
        {
            _socket = socket;
            _buffer = new byte[NetworkProtocol.GetCurrentProtocol().NetworkBufferLength];
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

            _socket?.Send(data);
        }
        public override void Send(NetworkMessage message)
        {
            Send(NetworkProtocol.GetCurrentProtocol().OnSerializeMessage(message));
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
                try
                {
                    _socket.Shutdown(SocketShutdown.Both);
                }
                catch { }

                _socket?.Disconnect(false);
                _reader?.Dispose();
            }

            if (!_disconnectionNotified)
            {
                _buffer = null;
                _socket = null;
                _disconnectionNotified = true;
                Disconnected?.Invoke(reason);
                Disconnected = null;
            }
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                Disconnect();

                IsDisposed = true;
            }
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

                    NetworkProtocol.ProcessParsing(this, _reader.Fill(data));
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