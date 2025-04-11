using Inertia.IO;
using Inertia.Logging;
using System;
using System.Net.Sockets;

namespace Inertia.Network
{
    public abstract class ConnectionEntity : NetworkEntity, IConnectionEntity, IDisposable
    {
        internal event EventHandler<ConnectionDisconnectingArgs>? Disconnecting;
        internal void OnDisconnecting(NetworkStopReason reason)
        {
            Disconnecting?.Invoke(this, new ConnectionDisconnectingArgs(this, reason));
        }

        private protected Socket _socket { get; private set; }
        private protected byte[] _buffer { get; }
        private DataReader _networkDataReader;

        protected ConnectionEntity(Socket socket, uint id, NetworkProtocol networkProtocol) : base(networkProtocol)
        {
            Id = id;
            ProcessingQueue = NetworkServerManager.GetBestProcessingQueue();
            Monitoring = new ConnectionMonitoring();
            _socket = socket;
            _networkDataReader = new DataReader();
            _buffer = new byte[networkProtocol.NetworkBufferLength];
        }

        public bool IsDisposed { get; private set; }
        public uint Id { get; }
        public object? State { get; set; }
        internal ServerProcessingQueue ProcessingQueue { get; }
        public ConnectionMonitoring Monitoring { get; }
        public bool IsConnected => _socket?.Connected == true;

        internal abstract void BeginReceiveMessages();
        private protected abstract void DoSend(byte[] data);

        public sealed override void Send(byte[] data)
        {
            Check.ThrowsIfDisposable(this, IsDisposed);

            if (!IsConnected)
            {
                throw new InvalidOperationException("Cannot send data if the client is not connected.");
            }

            try
            {
                DoSend(data);
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
    }
}