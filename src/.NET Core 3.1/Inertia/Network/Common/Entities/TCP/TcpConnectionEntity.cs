using System;
using System.Net.Sockets;

namespace Inertia.Network
{
    public sealed class TcpConnectionEntity : NetworkConnectionEntity, IDisposable
    {
        internal event BasicAction<NetworkDisconnectReason> Disconnected;

        public bool IsConnected => _socket != null && _socket.Connected;

        private readonly BasicReader _reader;
        private byte[] _buffer;
        private Socket _socket;
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

            try { _socket.Send(data); } catch { }
        }
        public override void Send(NetworkMessage message)
        {
            if (!IsConnected) return;
            Send(NetworkProtocol.GetCurrentProtocol().OnSerializeMessage(message));
        }

        public void Disconnect()
        {
            Disconnect(NetworkDisconnectReason.Manual);
        }
        public void Disconnect(NetworkDisconnectReason reason)
        {
            System.Threading.Tasks.Task.Run(() => {
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

                    _reader?.Dispose();
                    _socket?.Disconnect(false);
                }

                if (!_disconnectionNotified)
                {
                    _disconnectionNotified = true;
                    Disconnected?.Invoke(reason);

                    _buffer = null;
                    _socket = null;
                    Disconnected = null;
                }
            });
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
            try
            {
                var received = ((Socket)iar.AsyncState).EndReceive(iar);
                if (received == 0) throw new SocketException();

                NetworkProtocol.ProcessParsing(this, _reader.Fill(new ReadOnlySpan<byte>(_buffer, 0, received)));
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