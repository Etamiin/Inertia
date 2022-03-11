using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Inertia.Network
{
    public sealed class TcpConnectionEntity : NetworkConnectionEntity, IDisposable
    {
        internal event BasicAction<NetworkDisconnectReason> Disconnected;

        public bool IsConnected => _socket != null && _socket.Connected;

        private TcpServerEntity _server;
        private Socket _socket;
        private readonly BasicReader _reader;
        private byte[] _buffer;
        private bool _disconnectionNotified;
        private DateTime _spamTimer;
        private int _dataCountReceivedInLastSecond;

        internal TcpConnectionEntity(TcpServerEntity server, Socket socket, uint id) : base(id)
        {
            _server = server;
            _socket = socket;
            _buffer = new byte[NetworkProtocol.UsedProtocol.NetworkBufferLength];
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
            
            if (!IsConnected) return;

            try { _socket.Send(data); } catch { }
        }
        public override void Send(NetworkMessage message)
        {
            Send(NetworkProtocol.UsedProtocol.OnSerializeMessage(message));
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

                _reader?.Dispose();
                _socket?.Disconnect(false);
            }

            if (!_disconnectionNotified)
            {
                _disconnectionNotified = true;

                Disconnected?.Invoke(reason);
                _server.ConnectionDisconnected(this, reason);

                _server = null;
                _buffer = null;
                _socket = null;
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
        }

        private void OnReceiveData(IAsyncResult iar)
        {
            try
            {
                var received = ((Socket)iar.AsyncState).EndReceive(iar);
                if (received == 0)
                {
                    throw new SocketException();
                }

                _dataCountReceivedInLastSecond++;
                if (_spamTimer != null)
                {
                    var ts = DateTime.Now - _spamTimer;
                    if (ts.TotalSeconds > 1)
                    {
                        if (_dataCountReceivedInLastSecond >= NetworkProtocol.UsedProtocol.AuthorizedDataCountPerSecond)
                        {
                            Disconnect(NetworkDisconnectReason.SpammingMessages);
                            return;
                        }
                        else
                        {
                            _dataCountReceivedInLastSecond = 0;
                            _spamTimer = DateTime.Now;
                        }
                    }
                }

                NetworkProtocol.ProcessParsing(this, _reader.Fill(_buffer));
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