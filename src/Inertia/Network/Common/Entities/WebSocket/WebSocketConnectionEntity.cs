using Inertia.Logging;
using System;
using System.Diagnostics;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Inertia.Network
{
    public sealed class WebSocketConnectionEntity : TcpConnectionEntity
    {
        internal event BasicAction<WebSocketConnectionEntity>? ConnectionEstablished;

        public WebSocketConnectionState State { get; private set; }

        private SslStream? _sslStream;
        private X509Certificate? _serverCertificate;
        private DefaultWebSocketNetworkProtocol _wsProtocol;

        internal WebSocketConnectionEntity(Socket socket, uint id, NetworkProtocol protocol, X509Certificate? serverCertificate) : base(socket, id, protocol)
        {
            State = WebSocketConnectionState.Connecting;
            _wsProtocol = (DefaultWebSocketNetworkProtocol)protocol;
            _serverCertificate = serverCertificate;
            if (_serverCertificate != null)
            {
                _sslStream = new SslStream(new NetworkStream(socket), false);
            }
        }

        internal void SendSpecificOpCode(byte[] data, WebSocketOpCode opCode)
        {
            this.ThrowIfDisposable(IsDisposed);

            if (IsConnected)
            {
                try
                {
                    _sslStream?.Write(_wsProtocol.WriteMessage(data, opCode));
                }
                catch
                {
                    Disconnect(NetworkDisconnectReason.InvalidMessageSended);
                }
            }
        }
        internal void SendHandshakeResponse(string handshakeKey)
        {
            var httpResponse = $"HTTP/1.1 101 Switching Protocols\r\n" +
                $"Upgrade: websocket\r\n" +
                $"Connection: Upgrade\r\n" +
                $"Sec-WebSocket-Accept: {handshakeKey}\r\n\r\n";

            ProcessSend(Encoding.UTF8.GetBytes(httpResponse));

            State = WebSocketConnectionState.Connected;
            ConnectionEstablished?.Invoke(this);
        }

        internal protected override void BeginReceiveMessages()
        {
            if (_serverCertificate == null)
            {
                base.BeginReceiveMessages();
            }
            else
            {
                try
                {
                    _sslStream?.BeginAuthenticateAsServer(_serverCertificate, false, false, OnAuthentificated, _sslStream);
                }
                catch (Exception ex)
                {
                    SimpleLogger.Default.Error($"SSL Authentification Failed: {ex}");
                }
            }
        }

        private protected override void ProcessSend(byte[] data)
        {
            if (_sslStream == null)
            {
                if (State == WebSocketConnectionState.Connected)
                {
                    _socket.Send(_wsProtocol.WriteMessage(data, WebSocketOpCode.BinaryFrame));
                }
                else _socket.Send(data);
            }
            else if (State == WebSocketConnectionState.Connected)
            {
                _sslStream.Write(_wsProtocol.WriteMessage(data, WebSocketOpCode.BinaryFrame));
            }
            else _sslStream.Write(data);
        }
        private protected override void ProcessClean()
        {
            _sslStream?.Dispose();
            _sslStream = null;
            _serverCertificate = null;

            State = WebSocketConnectionState.Closed;
        }

        private void OnAuthentificated(IAsyncResult iar)
        {
            var sslStream = (SslStream)iar.AsyncState;

            sslStream.EndAuthenticateAsServer(iar);
            if (sslStream.IsAuthenticated)
            {
                sslStream.BeginRead(_buffer, 0, _buffer.Length, OnReceiveSslData, sslStream);
            }
            else
            {
                Disconnect(NetworkDisconnectReason.SslAuthentificationFailed);
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
                _sslStream?.BeginRead(_buffer, 0, _buffer.Length, OnReceiveSslData, _sslStream);
            }
        }
    }
}
