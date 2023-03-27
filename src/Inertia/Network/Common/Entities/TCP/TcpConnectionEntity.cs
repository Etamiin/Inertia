using Inertia.Logging;
using System;
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
        internal event BasicAction<TcpConnectionEntity, NetworkDisconnectReason> Disconnecting;

        public ConnectionStatistics Statistics { get; private set; }
        public bool IsDisposed { get; private set; }
        public bool IsConnected => _socket != null && _socket.Connected;

        internal protected byte[] Buffer { get; private set; }

        private Socket _socket;
        private BasicReader _reader;

        internal TcpConnectionEntity(Socket socket, uint id) : base(id)
        {
            Statistics = new ConnectionStatistics();

            _socket = socket;
            _reader = new BasicReader();
            Buffer = new byte[NetworkProtocol.Current.NetworkBufferLength];
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
                Disconnecting = null;

                _socket?.Disconnect(false);
                _reader?.Dispose();
                _socket = null;

                Buffer = null;

                return true;
            }

            return false;
        }
        public void Dispose()
        {
            Dispose(true);
        }

        internal protected virtual void BeginReceiveMessages()
        {
            _socket.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, OnReceiveData, _socket);
        }

        private void OnReceiveData(IAsyncResult iar)
        {
            try
            {
                var received = ((SslStream)iar.AsyncState).EndRead(iar);

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

                NetworkProtocol.ProcessParsing(this, _reader.Fill(new ReadOnlySpan<byte>(Buffer, 0, received)));
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
                _socket.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, OnReceiveData, _socket);
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