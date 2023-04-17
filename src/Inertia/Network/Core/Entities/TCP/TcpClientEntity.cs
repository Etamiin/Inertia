using Inertia.Logging;
using System;
using System.Net;
using System.Net.Sockets;

namespace Inertia.Network
{
    public abstract class TcpClientEntity : NetworkClientEntity, IDisposable
    {
        public sealed override bool IsConnected => _socket != null && _socket.Connected;

        public bool IsDisposed { get; private set; }

        private BasicReader? _networkDataReader;
        private Socket? _socket;
        private byte[] _buffer;
        
        protected TcpClientEntity(ClientParameters parameters) : base(parameters)
        {
        }

        public sealed override void Connect()
        {
            this.ThrowIfDisposable(IsDisposed);

            if (!IsConnected)
            {
                try
                {
                    _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    _socket.Connect(new IPEndPoint(IPAddress.Parse(_parameters.Ip), _parameters.Port));
                    
                    _networkDataReader = new BasicReader();
                    _buffer = new byte[Protocol.NetworkBufferLength];

                    Connected();
                    _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, OnReceiveData, _socket);
                }
                catch
                {
                    Disconnect(NetworkDisconnectReason.ConnectionFailed);
                }
            }
        }
        public sealed override bool Disconnect(NetworkDisconnectReason reason)
        {
            this.ThrowIfDisposable(IsDisposed);

            if (_socket != null)
            {
                Disconnecting(reason);

                if (_socket.Connected)
                {
                    _socket?.Disconnect(false);
                }

                _networkDataReader?.Dispose();
                _networkDataReader = null;
                _socket = null;
                _buffer = null;

                return true;
            }

            return false;
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
                try { 
                    _socket?.Send(dataToSend);
                }
                catch 
                {
                    Disconnect(NetworkDisconnectReason.InvalidMessageSended);
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
                int received = ((Socket)iar.AsyncState).EndReceive(iar);

                if (!IsConnected) return;
                if (received == 0)
                {
                    throw new SocketException((int)SocketError.SocketError);
                }

                NetworkProtocolFactory.ProcessParsing(Protocol, this, _networkDataReader.Fill(new ReadOnlySpan<byte>(_buffer, 0, received)));
            }
            catch (Exception e)
            {
                if (e is SocketException || e is ObjectDisposedException)
                {
                    Disconnect(NetworkDisconnectReason.ConnectionLost);
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
            if (IsDisposed) return;

            if (disposing)
            {
                Disconnect();
            }

            IsDisposed = true;
        }
    }
}