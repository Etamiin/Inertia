using System;
using System.Net.Sockets;

namespace Inertia.Network
{
    public sealed class TcpConnectionEntity : NetworkConnectionEntity, IDisposable
    {
        internal event BasicAction<TcpConnectionEntity, NetworkDisconnectReason> Disconnected;

        public ConnectionStatistics Statistics { get; private set; }
        public bool IsDisposed { get; private set; }
        public bool IsConnected => _socket != null && _socket.Connected;
        
        private TcpServerEntity _server;
        private Socket _socket;
        private BasicReader _reader;
        private byte[] _buffer;
        private DateTime? _spamTimer;

        internal TcpConnectionEntity(TcpServerEntity server, Socket socket, uint id) : base(id)
        {
            _server = server;
            _socket = socket;
            _buffer = new byte[NetworkProtocol.UsedProtocol.NetworkBufferLength];
            _reader = new BasicReader();
            Statistics = new ConnectionStatistics();
        }

        internal void StartReception()
        {
            if (!IsDisposed)
            {
                _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, OnReceiveData, _socket);
            }
        }

        public override void Send(byte[] data)
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
                Disconnect(NetworkDisconnectReason.InvalidMessage);
            }
        }
        public override void Send(NetworkMessage message)
        {
            Send(NetworkProtocol.UsedProtocol.OnSerializeMessage(message));
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
                finally
                {
                    Disconnected?.Invoke(this, reason);
                    _server.ConnectionDisconnected(this, reason);

                    _reader?.Dispose();
                    _socket?.Disconnect(false);
                    _server = null;
                    _buffer = null;
                    _socket = null;
                    Disconnected = null;
                }
            }
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
                if (messageReceivedInLastSecond >= NetworkProtocol.UsedProtocol.AuthorizedDataCountPerSecond)
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