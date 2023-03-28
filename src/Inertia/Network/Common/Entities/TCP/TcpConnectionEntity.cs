using Inertia.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

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

        internal TcpConnectionEntity(Socket socket, uint id) : base(id)
        {
            Statistics = new ConnectionStatistics();
            _socket = socket;
            _networkDataReader = new BasicReader();
            _buffer = new byte[NetworkProtocol.Current.NetworkBufferLength];
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
                InternalSend(data);
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

            if (_socket == null) return false;

            Disconnecting?.Invoke(this, reason);
            Disconnecting = null;

            _socket?.Disconnect(false);
            _networkDataReader?.Dispose();
            _socket = null;
            _buffer = null;

            InternalDisconnect();
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
        
        private protected virtual void InternalSend(byte[] data)
        {
            _socket.Send(data);
        }
        private protected virtual void InternalDisconnect() { }
        private protected void ProcessReceivedData(int receivedLength)
        {
            if (!IsConnected) return;
            if (receivedLength == 0)
            {
                throw new SocketException((int)SocketError.SocketError);
            }

            var messageReceivedInLastSecond = Statistics.NotifyMessageReceived();
            if (messageReceivedInLastSecond >= NetworkProtocol.Current.MaximumMessageCountPerSecond)
            {
                Disconnect(NetworkDisconnectReason.Spam);
                return;
            }

            NetworkProtocol.ProcessParsing(this, _networkDataReader.Fill(new ReadOnlySpan<byte>(_buffer, 0, receivedLength)));
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