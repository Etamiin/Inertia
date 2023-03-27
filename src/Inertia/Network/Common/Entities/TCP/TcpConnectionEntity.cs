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
        internal event BasicAction<TcpConnectionEntity, NetworkDisconnectReason>? Disconnecting;

        public ConnectionStatistics Statistics { get; private set; }
        public bool IsDisposed { get; private set; }
        public bool IsConnected => Socket != null && Socket.Connected;

        private protected Socket Socket { get; private set; }
        private protected BasicReader NetworkDataReader { get; private set; }
        private protected byte[] Buffer { get; private set; }

        internal TcpConnectionEntity(Socket socket, uint id) : base(id)
        {
            Statistics = new ConnectionStatistics();

            Socket = socket;
            NetworkDataReader = new BasicReader();
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

            if (IsConnected)
            {
                Disconnecting?.Invoke(this, reason);
                Disconnecting = null;

                Socket?.Disconnect(false);
                NetworkDataReader?.Dispose();
                Socket = null;
                Buffer = null;

                InternalDispose();
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
            Socket.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, OnReceiveData, Socket);
        }
        
        private protected virtual void InternalSend(byte[] data)
        {
            Socket.Send(data);
        }
        private protected virtual void InternalDispose() { }
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

            NetworkProtocol.ProcessParsing(this, NetworkDataReader.Fill(new ReadOnlySpan<byte>(Buffer, 0, receivedLength)));
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
                Socket.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, OnReceiveData, Socket);
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