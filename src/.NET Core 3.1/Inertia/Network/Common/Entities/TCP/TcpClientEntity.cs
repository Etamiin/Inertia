using System;
using System.Net;
using System.Net.Sockets;

namespace Inertia.Network
{
    public abstract class TcpClientEntity : NetworkClientEntity, IDisposable
    {
        public override bool IsConnected => _socket != null && _socket.Connected;

        private readonly byte[] _buffer;
        private BasicReader _reader;
        private Socket _socket;

        protected TcpClientEntity(string ip, int port) : base(ip, port)
        {
            _buffer = new byte[NetworkProtocol.GetCurrentProtocol().NetworkBufferLength];
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
                    _disconnectNotified = false;
                    _reader = new BasicReader();
                    _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    _socket.Connect(new IPEndPoint(IPAddress.Parse(_targetIp), _targetPort));

                    OnConnected();
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
                catch { }

                _reader?.Dispose();
                _socket?.Disconnect(false);
            }

            if (!_disconnectNotified)
            {
                _disconnectNotified = true;
                OnDisconnected(reason);
            }
        }
        public sealed override void Send(byte[] data)
        {
            if (IsDisposed)
            {
                throw new NotImplementedException(nameof(TcpClientEntity));
            }

            try { _socket.Send(data); } catch { }
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                BeforeDispose();
                Disconnect(NetworkDisconnectReason.Manual);
                _reader.Dispose();
                _socket?.Dispose();

                IsDisposed = true;
            }
        }

        private void OnReceiveData(IAsyncResult iar)
        {
            try
            {
                int received = ((Socket)iar.AsyncState).EndReceive(iar);
                if (received == 0) throw new SocketException();

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
                _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, OnReceiveData, _socket);
            }
        }
    }
}
