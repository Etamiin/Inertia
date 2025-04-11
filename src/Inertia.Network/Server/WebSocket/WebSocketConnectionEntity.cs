using Inertia.Logging;
using System;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Inertia.Network
{
    public sealed class WebSocketConnectionEntity : TcpConnectionEntityBase
    {
        internal event WebSocketConnectionEstablished? ConnectionEstablished;

        private WebSocketNetworkProtocol _webSocketProtocol;
        private SslStream? _sslStream;
        private X509Certificate? _serverCertificate;

        internal WebSocketConnectionEntity(Socket socket, uint id, X509Certificate? serverCertificate) : base(socket, id, NetworkManager.WsProtocol)
        {
            _webSocketProtocol = (WebSocketNetworkProtocol)NetworkProtocol;
            _serverCertificate = serverCertificate;

            if (_serverCertificate != null)
            {
                _sslStream = new SslStream(new NetworkStream(socket), false);
            }

            ConnectionState = WebSocketConnectionState.Connecting;
        }

        public WebSocketConnectionState ConnectionState { get; private set; }

        internal override void BeginReceiveMessages()
        {
            if (_serverCertificate is null)
            {
                base.BeginReceiveMessages();
            }
            else
            {
                try
                {
                    _sslStream?.BeginAuthenticateAsServer(_serverCertificate, false, false, OnAuthenticated, _sslStream);
                }
                catch (Exception ex)
                {
                    LoggingProvider.LogHandler.Log(LogLevel.Error, $"WebSocket connection failed on SSL authentication.", ex);
                }
            }
        }
        private protected override void DoSend(byte[] data)
        {
            if (_sslStream is null)
            {
                if (ConnectionState == WebSocketConnectionState.Connected)
                {
                    _socket.Send(_webSocketProtocol.WriteWebSocketMessage(data, WebSocketOpCode.BinaryFrame));
                }
                else _socket.Send(data);
            }
            else
            {
                if (ConnectionState == WebSocketConnectionState.Connected)
                {
                    _sslStream.Write(_webSocketProtocol.WriteWebSocketMessage(data, WebSocketOpCode.BinaryFrame));
                }
                else _sslStream.Write(data);
            }
        }

        internal void SendSpecificOpCode(byte[] data, WebSocketOpCode opCode)
        {
            this.ThrowIfDisposable(IsDisposed);

            if (IsConnected)
            {
                try
                {
                    _sslStream?.Write(_webSocketProtocol.WriteWebSocketMessage(data, opCode));
                }
                catch (Exception ex)
                {
                    LoggingProvider.LogHandler.Log(LogLevel.Error, $"An error occurred when WebSocket OpCode message.", ex);

                    Disconnect(NetworkStopReason.InvalidMessageSent);
                }
            }
        }
        internal void SendHandshakeResponse(string handshakeKey)
        {
            var httpResponse = $"HTTP/1.1 101 Switching Protocols\r\n" +
                $"Upgrade: websocket\r\n" +
                $"Connection: Upgrade\r\n" +
                $"Sec-WebSocket-Accept: {handshakeKey}\r\n\r\n";

            DoSend(Encoding.UTF8.GetBytes(httpResponse));

            ConnectionState = WebSocketConnectionState.Connected;
            ConnectionEstablished?.Invoke(this);
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed && disposing)
            {
                _sslStream?.Dispose();
                _sslStream = null;
                _serverCertificate = null;

                ConnectionState = WebSocketConnectionState.Closed;
            }

            base.Dispose(disposing);
        }

        private void OnAuthenticated(IAsyncResult iar)
        {
            var sslStream = (SslStream)iar.AsyncState;

            sslStream.EndAuthenticateAsServer(iar);
            if (sslStream.IsAuthenticated)
            {
                sslStream.BeginRead(_buffer, 0, _buffer.Length, OnReceiveSslData, sslStream);
            }
            else
            {
                Disconnect(NetworkStopReason.SslAuthenticationFailed);
            }
        }
        private void OnReceiveSslData(IAsyncResult iar)
        {
            try
            {
                var received = ((SslStream)iar.AsyncState).EndRead(iar);
                ProcessReceivedData(received);
            }
            catch (Exception ex)
            {
                LoggingProvider.LogHandler.Log(LogLevel.Error, $"An error occurred when receiving data (WebSocket SSL stream)", ex);

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
                _sslStream?.BeginRead(_buffer, 0, _buffer.Length, OnReceiveSslData, _sslStream);
            }
        }
    }
}