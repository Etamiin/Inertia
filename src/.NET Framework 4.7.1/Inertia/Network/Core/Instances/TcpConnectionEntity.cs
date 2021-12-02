using System;
using System.Net.Sockets;

namespace Inertia.Network
{
<<<<<<< HEAD
    public sealed class TcpConnectionEntity : NetworkConnectionEntity
    {
        internal BasicAction<NetworkDisconnectReason> Disconnected { get; set; }

        public bool IsConnected => _socket != null && _socket.Connected;

        internal readonly uint Id;

=======
    public sealed class TcpConnectionEntity : NetworkConnectionEntity, IDisposable
    {
        internal event BasicAction<NetworkDisconnectReason> Disconnected;

        public bool IsConnected => _socket != null && _socket.Connected;

>>>>>>> premaster
        private byte[] _buffer;
        private Socket _socket;
        private BasicReader _reader;
        private bool _disconnectionNotified;
        
<<<<<<< HEAD
        internal TcpConnectionEntity(Socket socket, uint id)
        {
            Id = id;
            _socket = socket;
            _buffer = new byte[NetworkProtocol.NetworkBufferLength];
=======
        internal TcpConnectionEntity(Socket socket, uint id) : base(id)
        {
            _socket = socket;
            _buffer = new byte[NetworkProtocol.GetCurrentProtocol().NetworkBufferLength];
>>>>>>> premaster
            _reader = new BasicReader();
        }

        internal void StartReception()
        {
            if (!IsDisposed)
            {
                _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(OnReceiveData), _socket);
            }
        }

        public override void Send(byte[] data)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(TcpConnectionEntity));
            }

<<<<<<< HEAD
            try { _socket?.Send(data); } catch { }
        }
        public override void Send(NetworkMessage message)
        {
            Send(NetworkProtocol.GetProtocol().OnParseMessage(message));
=======
            _socket?.Send(data);
        }
        public override void Send(NetworkMessage message)
        {
            Send(NetworkProtocol.GetCurrentProtocol().OnSerializeMessage(message));
>>>>>>> premaster
        }

        public void Disconnect()
        {
            Disconnect(NetworkDisconnectReason.Manual);
        }
        public void Disconnect(NetworkDisconnectReason reason)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(TcpConnectionEntity));
            }

            if (IsConnected)
            {
                try
                {
                    _socket.Shutdown(SocketShutdown.Both);
                }
                catch { }

                _socket?.Disconnect(false);
                _reader?.Dispose();
            }
<<<<<<< HEAD
            if (!_disconnectionNotified)
            {

                _reader = null;
=======

            if (!_disconnectionNotified)
            {
>>>>>>> premaster
                _buffer = null;
                _socket = null;
                _disconnectionNotified = true;
                Disconnected?.Invoke(reason);
<<<<<<< HEAD
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    Disconnect();
                    Disconnected = null;
                }
            }

            base.Dispose(disposing);
=======
                Disconnected = null;
            }
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                Disconnect();

                IsDisposed = true;
            }
>>>>>>> premaster
        }

        private void OnReceiveData(IAsyncResult iar)
        {
            if (IsConnected)
            {
                try
                {
                    var received = ((Socket)iar.AsyncState).EndReceive(iar);
                    if (received == 0)
                    {
                        throw new SocketException();
                    }

                    var data = new byte[received];
                    Array.Copy(_buffer, data, received);

<<<<<<< HEAD
                    NetworkProtocol.GetProtocol().OnReceiveData(this, _reader.Fill(data));
=======
                    NetworkProtocol.ProcessParsing(this, _reader.Fill(data));
>>>>>>> premaster
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
                    _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(OnReceiveData), _socket);
                }
            }
        }
    }
<<<<<<< HEAD
}
=======
}
>>>>>>> premaster
