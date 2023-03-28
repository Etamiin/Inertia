using Inertia.Logging;
using System;
using System.Diagnostics;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Inertia.Network
{
    public class WebSocketConnectionEntity : TcpConnectionEntity
    {
        internal event BasicAction<WebSocketConnectionEntity>? ConnectionEstablished;

        public WebSocketConnectionState State { get; private set; }

        private SslStream? _sslStream;
        private X509Certificate? _serverCertificate;

        internal WebSocketConnectionEntity(Socket socket, uint id, X509Certificate? serverCertificate) : base(socket, id)
        {
            State = WebSocketConnectionState.Connecting;

            _serverCertificate = serverCertificate;
            if (_serverCertificate != null)
            {
                _sslStream = new SslStream(new NetworkStream(socket), false);
                Logger.Default.Warn("ssl stream OK");
            }
        }

        internal void SendSpecificOpCode(byte[] data, WebSocketOpCode opCode)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(TcpConnectionEntity));
            }

            if (!IsConnected) return;

            try
            {
                var wsProtocol = (WebSocketNetworkProtocol)NetworkProtocol.Current;
                _sslStream?.Write(wsProtocol.WriteWsMessage(data, opCode));
            }
            catch
            {
                Disconnect(NetworkDisconnectReason.InvalidMessageSended);
            }

        }
        internal void SendHandshakeResponse(string handshakeKey)
        {
            var response = $"HTTP/1.1 101 Switching Protocols\r\n" +
                $"Upgrade: websocket\r\n" +
                $"Connection: Upgrade\r\n" +
                $"Sec-WebSocket-Accept: {handshakeKey}\r\n\r\n";

            InternalSend(Encoding.UTF8.GetBytes(response));

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
                _sslStream?.BeginAuthenticateAsServer(_serverCertificate, false, false, OnAuthentificated, _sslStream);
            }
        }

        private protected override void InternalSend(byte[] data)
        {
            if (_sslStream == null)
            {
                if (State == WebSocketConnectionState.Connected)
                {
                    var wsProtocol = (WebSocketNetworkProtocol)NetworkProtocol.Current;
                    _socket.Send(wsProtocol.WriteWsMessage(data, WebSocketOpCode.BinaryFrame));
                }
                else
                {
                    _socket.Send(data);
                }
            }
            else if (State == WebSocketConnectionState.Connected)
            {
                Logger.Default.Warn("sending ssl data as authentified");

                var wsProtocol = (WebSocketNetworkProtocol)NetworkProtocol.Current;
                _sslStream.Write(wsProtocol.WriteWsMessage(data, WebSocketOpCode.BinaryFrame));
            }
            else
            {
                Logger.Default.Warn("sending normal ssl data");

                _sslStream.Write(data);
            }
        }
        private protected override void InternalDisconnect()
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
                Logger.Default.Warn("PROCESSING: " + received + " <> " + State);

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
                _sslStream.BeginRead(_buffer, 0, _buffer.Length, OnReceiveSslData, _sslStream);
            }
        }
    }
}
