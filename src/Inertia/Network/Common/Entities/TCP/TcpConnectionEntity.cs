using System;
using System.Net.Sockets;

namespace Inertia.Network
{
    public sealed class TcpConnectionEntity : NetworkConnectionEntity, IDisposable
    {
        internal event BasicAction<TcpConnectionEntity, NetworkDisconnectReason> Disconnecting;

        public ConnectionStatistics Statistics { get; private set; }
        public bool IsDisposed { get; private set; }
        public bool IsConnected => _socket != null && _socket.Connected;
        
        private Socket _socket;
        private BasicReader _reader;
        private byte[] _buffer;
        private DateTime? _spamTimer;

        internal TcpConnectionEntity(Socket socket, uint id) : base(id)
        {
            _socket = socket;
            _buffer = new byte[NetworkProtocol.Current.NetworkBufferLength];
            _reader = new BasicReader();
            Statistics = new ConnectionStatistics();
        }

        internal void BeginReceiveMessages()
        {
            if (!IsDisposed)
            {
                _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, OnReceiveData, _socket);
            }
        }

        public sealed override void Send(byte[] data)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(TcpConnectionEntity));
            }

            if (!IsConnected) return;

            if (data == null || data.Length == 0)
            {
                throw new ArgumentNullException(nameof(data));
            }

            try
            {
                _socket.Send(data);
            }
            catch 
            {
                Disconnect(NetworkDisconnectReason.InvalidMessageSended);
            }
        }
        public sealed override void Send(NetworkMessage message)
        {
            Send(NetworkProtocol.Current.SerializeMessage(message));
        }

        public bool Disconnect()
        {
            return Disconnect(NetworkDisconnectReason.Manual);
        }
        public bool Disconnect(NetworkDisconnectReason reason)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(TcpConnectionEntity));
            }

            if (IsConnected)
            {
                Disconnecting?.Invoke(this, reason);

                _socket?.Disconnect(false);
                _reader?.Dispose();
                _buffer = null;
                _socket = null;
                Disconnecting = null;

                return true;
            }

            return false;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void OnReceiveData(IAsyncResult iar)
        {
            try
            {
                var received = ((Socket)iar.AsyncState).EndReceive(iar);

                if (!IsConnected) return;

                if (received == 0)
                {
                    throw new SocketException((int)SocketError.SocketError);
                }

                var messageReceivedInLastSecond = Statistics.NotifyMessageReceived();
                if (messageReceivedInLastSecond >= NetworkProtocol.Current.MaximumMessageCountPerSecond)
                {
                    Disconnect(NetworkDisconnectReason.Spam);
                    return;
                }

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
                _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, OnReceiveData, _socket);
            }
        }
        private void Dispose(bool disposing)
        {
            if (IsDisposed) return;

            if (disposing)
            {
                Disconnect();
            }

            IsDisposed = true;
        }
    
    }
}