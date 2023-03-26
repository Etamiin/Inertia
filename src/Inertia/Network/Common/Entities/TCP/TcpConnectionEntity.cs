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
    public sealed class TcpConnectionEntity : NetworkConnectionEntity, IDisposable
    {
        internal event BasicAction<TcpConnectionEntity, NetworkDisconnectReason> Disconnecting;
        internal event BasicAction<TcpConnectionEntity> WebSocketDetermined;

        public ConnectionStatistics Statistics { get; private set; }
        public bool IsDisposed { get; private set; }
        public bool IsConnected => _socket != null && _socket.Connected;
        public bool? IsWebSocketConnection { get; internal set; }

        private Socket _socket;
        private BasicReader _reader;
        private SslStream? _sslStream;
        private byte[] _buffer;
        private DateTime? _spamTimer;
        
        internal TcpConnectionEntity(Socket socket, uint id) : base(id)
        {
            Statistics = new ConnectionStatistics();

            _socket = socket;
            _buffer = new byte[NetworkProtocol.Current.NetworkBufferLength];
            _reader = new BasicReader();

            if (NetworkProtocol.IsCurrentWebSocketProtocol && NetworkProtocol._sslCertificate != null)
            {
                _sslStream = new SslStream(new NetworkStream(socket), false);
            }
        }

        internal void SetAsWebSocketConnection()
        {
            IsWebSocketConnection = true;
            WebSocketDetermined?.Invoke(this);
        }
        internal void BeginReceiveMessages()
        {
            if (!IsDisposed)
            {
                if (_sslStream == null)
                {
                    _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, OnReceiveData, _socket);
                }
                else
                {
                    _sslStream?.BeginAuthenticateAsServer(NetworkProtocol._sslCertificate, false, false, OnAuthentificated, _sslStream);
                }
            }
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
                if (_sslStream == null)
                {
                    if (!IsWebSocketConnection.HasValue || IsWebSocketConnection == false)
                    {
                        _socket.Send(data);
                    }
                    else
                    {
                        var wsProtocol = (WebSocketNetworkProtocol)NetworkProtocol.Current;
                        _socket.Send(wsProtocol.WriteWsMessage(data, WebSocketOpCode.BinaryFrame));
                    }
                }
                else
                {
                    if (!IsWebSocketConnection.HasValue || IsWebSocketConnection == false)
                    {
                        _sslStream.Write(data);
                    }
                    else
                    {
                        var wsProtocol = (WebSocketNetworkProtocol)NetworkProtocol.Current;
                        _sslStream.Write(wsProtocol.WriteWsMessage(data, WebSocketOpCode.BinaryFrame));
                    }
                }
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

        internal void Send(byte[] data, WebSocketOpCode opCode)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(TcpConnectionEntity));
            }

            if (!IsConnected) return;

            try
            {
                var wsProtocol = (WebSocketNetworkProtocol)NetworkProtocol.Current;
                _sslStream?.WriteAsync(wsProtocol.WriteWsMessage(data, opCode));
            }
            catch
            {
                Disconnect(NetworkDisconnectReason.InvalidMessageSended);
            }

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

                _socket?.Disconnect(false);
                _reader?.Dispose();
                _sslStream?.Dispose();
                _buffer = null;
                _socket = null;
                _sslStream = null;

                return true;
            }

            return false;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void OnAuthentificated(IAsyncResult iar)
        {
            var sslStream = (SslStream)iar.AsyncState;

            sslStream.EndAuthenticateAsServer(iar);
            if (sslStream.IsAuthenticated)
            {
                sslStream.BeginRead(_buffer, 0, _buffer.Length, OnReceiveData, sslStream);
            }
            else
            {
                Disconnect(NetworkDisconnectReason.SslAuthentificationFailed);
            }
        }
        private void OnReceiveData(IAsyncResult iar)
        {
            try
            {
                var received = 0;

                if (_sslStream == null)
                {
                    received = ((Socket)iar.AsyncState).EndReceive(iar);
                }
                else
                {
                    received = ((SslStream)iar.AsyncState).EndRead(iar);
                }

                if (!IsConnected) return;
                if (received == 0)
                {
                    throw new SocketException((int)SocketError.SocketError);
                }

                var messageReceivedInLastSecond = Statistics.NotifyMessageReceived();
                if (messageReceivedInLastSecond >= NetworkProtocol.Current.MaximumMessageCountPerSecond)
                {
                    Disconnect(NetworkDisconnectReason.Spam);
                    return;
                }

                NetworkProtocol.ProcessParsing(this, _reader.Fill(new ReadOnlySpan<byte>(_buffer, 0, received)));
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
                if (_sslStream == null)
                {
                    _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, OnReceiveData, _socket);
                }
                else
                {
                    _sslStream.BeginRead(_buffer, 0, _buffer.Length, OnReceiveData, _sslStream);
                }
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