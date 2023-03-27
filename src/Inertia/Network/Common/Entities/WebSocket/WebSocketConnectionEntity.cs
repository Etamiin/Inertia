using System;
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

        private SslStream _sslStream;
        private X509Certificate? _serverCertificate;

        internal WebSocketConnectionEntity(Socket socket, uint id, X509Certificate? serverCertificate) : base(socket, id)
        {
            State = WebSocketConnectionState.Connecting;

            _serverCertificate = serverCertificate;
            if (_serverCertificate != null)
            {
                _sslStream = new SslStream(new NetworkStream(socket), false);
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
                _sslStream?.WriteAsync(wsProtocol.WriteWsMessage(data, opCode));
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

            Send(Encoding.UTF8.GetBytes(response));

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
                var wsProtocol = (WebSocketNetworkProtocol)NetworkProtocol.Current;
                Socket.Send(wsProtocol.WriteWsMessage(data, WebSocketOpCode.BinaryFrame));
            }
            else if (_sslStream.IsAuthenticated)
            {
                var wsProtocol = (WebSocketNetworkProtocol)NetworkProtocol.Current;
                _sslStream.Write(wsProtocol.WriteWsMessage(data, WebSocketOpCode.BinaryFrame));
            }
            else
            {
                _sslStream.Write(data);
            }
        }
        private protected override void InternalDispose()
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
                sslStream.BeginRead(Buffer, 0, Buffer.Length, OnReceiveSslData, sslStream);
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
                _sslStream.BeginRead(Buffer, 0, Buffer.Length, OnReceiveSslData, _sslStream);
            }
        }
    }
}
