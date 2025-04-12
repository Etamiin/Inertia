using Inertia.IO;
using Inertia.Logging;
using System;
using System.Net.Sockets;

namespace Inertia.Network
{
    public class TcpConnectionEntity : NetworkEntity, IConnectionEntity, IDisposable
    {
        internal event EventHandler<ConnectionDisconnectingArgs>? Disconnecting;
        internal void OnDisconnecting(NetworkStopReason reason)
        {
            Disconnecting?.Invoke(this, new ConnectionDisconnectingArgs(this, reason));
        }

        private protected Socket _socket { get; private set; }
        private protected byte[] _buffer { get; }
        private protected DataReader _networkDataReader;

        public TcpConnectionEntity(Socket socket, uint id) : this(socket, id, NetworkManager.TcpProtocol)
        {
        }
        protected TcpConnectionEntity(Socket socket, uint id, NetworkProtocol networkProtocol) : base(networkProtocol)
        {
            Id = id;
            ProcessingQueue = NetworkServerManager.GetBestProcessingQueue();
            Monitoring = new ConnectionMonitoring();

            _socket = socket;
            _networkDataReader = new DataReader();
            _buffer = new byte[networkProtocol.NetworkBufferLength];
        }

        internal ServerProcessingQueue ProcessingQueue { get; }
        public bool IsDisposed { get; private set; }
        public uint Id { get; }
        public object? State { get; set; }
        public ConnectionMonitoring Monitoring { get; }
        public bool IsConnected => _socket?.Connected == true;
        
        internal virtual void BeginReceiveMessages()
        {
            _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, OnReceiveData, _socket);
        }

        public override void Send(byte[] data)
        {
            Check.ThrowsIfDisposable(this, IsDisposed);

            if (!IsConnected)
            {
                throw new InvalidOperationException("Cannot send data if the client is not connected.");
            }

            try
            {
                _socket.Send(data);
            }
            catch (Exception ex)
            {
                LoggingProvider.LogHandler.Log(LogLevel.Error, $"An error occurred when sending message.", ex);

                Disconnect(NetworkStopReason.InvalidMessageSent);
            }
        }
        public override void Disconnect(NetworkStopReason reason)
        {
            Check.ThrowsIfDisposable(this, IsDisposed);

            if (!IsConnected)
            {
                throw new InvalidOperationException("Client is not connected.");
            }

            OnDisconnecting(reason);
            Disconnecting = null;

            _socket?.Disconnect(false);
            _networkDataReader?.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed && disposing)
            {
                Disconnect();

                IsDisposed = true;
            }
        }
        protected virtual void OnReceiveData(IAsyncResult iar)
        {
            try
            {
                if (!IsConnected) return;

                var receivedLength = ((Socket)iar.AsyncState).EndReceive(iar);
                if (receivedLength == 0 && !IsDisposed)
                {
                    Disconnect(NetworkStopReason.ConnectionLost);
                    return;
                }

                ProcessReceivedData(receivedLength);
            }
            catch (Exception ex)
            {
                LoggingProvider.LogHandler.Log(LogLevel.Error, $"An error occurred when receiving socket data", ex);

                if (ex is SocketException || ex is ObjectDisposedException)
                {
                    if (!IsDisposed)
                    {
                        Disconnect(NetworkStopReason.ConnectionLost);
                        return;
                    }
                }
            }

            if (IsConnected)
            {
                _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, OnReceiveData, _socket);
            }
        }
        private protected void ProcessReceivedData(int receivedLength)
        {
            if (!IsConnected || receivedLength == 0) return;

            Monitoring.NotifyMessageReceived();
            if (Monitoring.MessageReceivedInLastSecond >= NetworkProtocol.MaxReceivedMessagePerSecondPerClient)
            {
                Disconnect(NetworkStopReason.Spam);
                return;
            }

            _networkDataReader.Lock((reader) =>
            {
                reader.Fill(_buffer, receivedLength);
            });

            ProcessingQueue.Enqueue(() => _networkDataReader.Lock((reader) =>
            {
                NetworkManager.ParseAndHandle(this, reader);
            }));
        }
    }
}