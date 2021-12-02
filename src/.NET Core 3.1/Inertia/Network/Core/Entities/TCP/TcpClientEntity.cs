using System;
using System.Net;
using System.Net.Sockets;

namespace Inertia.Network
{
    public abstract class TcpClientEntity : NetworkClientEntity, IDisposable
    {
        public override bool IsConnected => _socket != null && _socket.Connected;

        private readonly byte[] _buffer;
        private readonly BasicReader _reader;
        private Socket _socket;

        public TcpClientEntity(string ip, int port) : base(ip, port)
        {
            _buffer = new byte[NetworkProtocol.GetCurrentProtocol().NetworkBufferLength];
            _reader = new BasicReader();
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

                _socket?.Disconnect(false);
            }
            if (!_disconnectNotified)
            {
                _reader.Clear();
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

            if (IsConnected)
            {
                _socket.Send(data);
            }
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
                if (received == 0)
                {
                    throw new SocketException();
                }

                var data = new byte[received];
                Array.Copy(_buffer, data, received);

                NetworkProtocol.ProcessParsing(this, _reader.Fill(data));
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
