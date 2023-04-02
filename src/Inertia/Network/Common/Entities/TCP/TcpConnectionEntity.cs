using System;
using System.Net.Sockets;

namespace Inertia.Network
{
    public class TcpConnectionEntity : NetworkConnectionEntity, IDisposable
    {
        internal event BasicAction<TcpConnectionEntity, NetworkDisconnectReason>? Disconnecting;

        public ConnectionStatistics Statistics { get; private set; }
        public bool IsDisposed { get; private set; }
        public bool IsConnected => _socket != null && _socket.Connected;

        private protected Socket _socket { get; private set; }
        private protected BasicReader _networkDataReader { get; private set; }
        private protected byte[] _buffer { get; private set; }

        internal TcpConnectionEntity(Socket socket, uint id, NetworkProtocol protocol) : base(id, protocol)
        {
            Statistics = new ConnectionStatistics();
            _socket = socket;
            _networkDataReader = new BasicReader();
            _buffer = new byte[_protocol.NetworkBufferLength];
        }

        public sealed override void Send(byte[] dataToSend)
        {
            this.ThrowIfDisposable(IsDisposed);

            if (dataToSend == null || dataToSend.Length == 0)
            {
                throw new ArgumentNullException(nameof(dataToSend));
            }

            if (IsConnected)
            {
                try
                {
                    ProcessSend(dataToSend);
                }
                catch
                {
                    Disconnect(NetworkDisconnectReason.InvalidMessageSended);
                }
            }
        }

        public bool Disconnect()
        {
            return Disconnect(NetworkDisconnectReason.Manual);
        }
        public override bool Disconnect(NetworkDisconnectReason reason)
        {
            this.ThrowIfDisposable(IsDisposed);

            if (_socket != null)
            {
                Disconnecting?.Invoke(this, reason);
                Disconnecting = null;

                if (IsConnected)
                {
                    _socket?.Disconnect(false);
                }

                _networkDataReader?.Dispose();
                _networkDataReader = null;
                _socket = null;
                _buffer = null;
            }

            ProcessClean();
            return true;
        }
        public void Dispose()
        {
            Dispose(true);
        }

        internal protected virtual void BeginReceiveMessages()
        {
            _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, OnReceiveData, _socket);
        }
        
        private protected virtual void ProcessSend(byte[] data)
        {
            _socket.Send(data);
        }
        private protected virtual void ProcessClean() { }
        private protected void ProcessReceivedData(int receivedLength)
        {
            if (!IsConnected) return;
            if (receivedLength == 0)
            {
                throw new SocketException((int)SocketError.SocketError);
            }

            Statistics.NotifyMessageReceived();
            if (Statistics.MessageReceivedInLastSecond >= _protocol.MaximumMessageCountPerSecond)
            {
                Disconnect(NetworkDisconnectReason.Spam);
                return;
            }

            NetworkProtocolFactory.ProcessParsing(_protocol, this, _networkDataReader.Fill(new ReadOnlySpan<byte>(_buffer, 0, receivedLength)));
        }

        private void OnReceiveData(IAsyncResult iar)
        {
            try
            {
                var received = ((Socket)iar.AsyncState).EndReceive(iar);
                ProcessReceivedData(received);
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