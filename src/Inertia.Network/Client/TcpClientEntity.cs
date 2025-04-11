using Inertia.IO;
using Inertia.Logging;
using System;
using System.Net;
using System.Net.Sockets;

namespace Inertia.Network
{
    public abstract class TcpClientEntity : ClientEntity, IDisposable
    {
        private DataReader? _networkDataReader;
        private Socket? _socket;
        private byte[]? _buffer;
        
        protected TcpClientEntity(string ip, int port, ClientProcessingQueue processingQueue) : base(ip, port, NetworkManager.TcpProtocol, processingQueue)
        {
        }

        public bool IsDisposed { get; private set; }
        public sealed override bool IsConnected => _socket != null && _socket.Connected;

        public sealed override void Connect()
        {
            Check.ThrowsIfDisposable(this, IsDisposed);

            if (IsConnected)
            {
                throw new InvalidOperationException("Client is already connected.");
            }

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Connect(new IPEndPoint(IPAddress.Parse(Ip), Port));

            _networkDataReader = new DataReader();
            _buffer = new byte[NetworkProtocol.NetworkBufferLength];

            OnConnected();
            _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, OnReceiveData, _socket);
        }
        public sealed override void Disconnect(NetworkStopReason reason)
        {
            Check.ThrowsIfDisposable(this, IsDisposed);

            if (_socket == null)
            {
                throw new InvalidOperationException("Client is not connected.");
            }

            OnDisconnecting(reason);

            if (_socket.Connected)
            {
                _socket?.Disconnect(false);
            }

            _networkDataReader?.Dispose();
            _networkDataReader = null;
            _socket = null;
            _buffer = null;
        }
        public sealed override void Send(byte[] data)
        {
            Check.ThrowsIfDisposable(this, IsDisposed);

            if (!IsConnected)
            {
                throw new InvalidOperationException("Cannot send data if the client is not connected.");
            }

            try
            {
                _socket?.Send(data);
            }
            catch (Exception ex)
            {
                LoggingProvider.LogHandler.Log(LogLevel.Error, "Sending data from client failed.", ex);

                Disconnect(NetworkStopReason.InvalidMessageSent);
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
                if (!IsConnected) return;

                int receivedLength = ((Socket)iar.AsyncState).EndReceive(iar);
                if (receivedLength == 0 && !IsDisposed)
                {
                    Disconnect(NetworkStopReason.ConnectionLost);
                    return;
                }

                _networkDataReader.Lock((reader) =>
                {
                    _networkDataReader.Fill(_buffer!, receivedLength);
                });

                _processingQueue.Enqueue(() => _networkDataReader.Lock((reader) =>
                {
                    NetworkManager.ParseAndHandle(this, reader);
                }));
            }
            catch (Exception ex)
            {
                LoggingProvider.LogHandler.Log(LogLevel.Error, "An error occurred when receiving network data", ex);

                if (ex is SocketException se || ex is ObjectDisposedException)
                {
                    Disconnect(NetworkStopReason.ConnectionLost);
                    return;
                }
            }

            if (IsConnected)
            {
                _socket?.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, OnReceiveData, _socket);
            }
        }
        private void Dispose(bool disposing)
        {
            if (!IsDisposed && disposing)
            {
                Disconnect();

                IsDisposed = true;
            }
        }
    }
}