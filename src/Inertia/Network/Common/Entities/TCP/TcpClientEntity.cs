using System;
using System.Net;
using System.Net.Sockets;

namespace Inertia.Network
{
    public abstract class TcpClientEntity : NetworkClientEntity, IDisposable
    {
        public sealed override bool IsConnected => _socket != null && _socket.Connected;

        public bool IsDisposed { get; private set; }

        private readonly byte[] _buffer;
        private BasicReader? _reader;
        private Socket? _socket;

        protected TcpClientEntity(string ip, int port) : base(ip, port)
        {
            _buffer = new byte[NetworkProtocol.UsedProtocol.NetworkBufferLength];
        }
        
        public sealed override void Connect()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(TcpClientEntity));
            }

            if (!IsConnected)
            {
                try
                {
                    _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    _socket.Connect(new IPEndPoint(IPAddress.Parse(_ip), _port));
                    
                    _reader = new BasicReader();

                    Connected();
                    _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, OnReceiveData, _socket);
                }
                catch
                {
                    Disconnect(NetworkDisconnectReason.ConnectionFailed);
                }
            }
        }
        public sealed override void Disconnect(NetworkDisconnectReason reason)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(TcpClientEntity));
            }

            if (IsConnected)
            {
                try
                {
                    _socket?.Shutdown(SocketShutdown.Both);
                }
                finally
                {
                    _reader?.Dispose();
                    _socket?.Disconnect(false);

                    Disconnected(reason);
                }
            }
        }
        public sealed override void Send(byte[] data)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(TcpClientEntity));
            }

            if (data == null || data.Length == 0)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (IsConnected)
            {
                try { 
                    _socket?.Send(data);
                }
                catch 
                {
                    Disconnect(NetworkDisconnectReason.InvalidMessage);
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

                NetworkProtocol.ProcessParsing(this, _reader.Fill(new ReadOnlySpan<byte>(_buffer, 0, received)));
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
