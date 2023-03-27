using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Inertia.Network
{
    public class WebSocketConnectionEntity : TcpConnectionEntity
    {
        public WebSocketConnectionState State { get; private set; }

        private SslStream _sslStream;
        private X509Certificate? _serverCertificate;

        internal WebSocketConnectionEntity(Socket socket, uint id, X509Certificate? serverCertificate) : base(socket, id)
        {
            _sslStream = new SslStream(new NetworkStream(socket), false);
            _serverCertificate = serverCertificate;
            State = WebSocketConnectionState.Connecting;
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
                    var wsProtocol = (WebSocketNetworkProtocol)NetworkProtocol.Current;
                    _socket.Send(wsProtocol.WriteWsMessage(data, WebSocketOpCode.BinaryFrame));
                }
                else
                {
                    if (_sslStream.IsAuthenticated)
                    {
                        var wsProtocol = (WebSocketNetworkProtocol)NetworkProtocol.Current;
                        _sslStream.Write(wsProtocol.WriteWsMessage(data, WebSocketOpCode.BinaryFrame));
                    }
                    else
                    {
                        _sslStream.Write(data);
                    }
                }
            }
            catch
            {
                Disconnect(NetworkDisconnectReason.InvalidMessageSended);
            }
        }
        public bool Disconnect(NetworkDisconnectReason reason)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(TcpConnectionEntity));
            }

            if (IsConnected)
            {
                _sslStream?.Dispose();
                _sslStream = null;
                _serverCertificate = null;

                State = WebSocketConnectionState.Closed;

                return true;
            }

            return false;
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

        internal void SendHandshakeResponse(string handshakeKey)
        {
            var response = $"HTTP/1.1 101 Switching Protocols\r\n" +
                $"Upgrade: websocket\r\n" +
                $"Connection: Upgrade\r\n" +
                $"Sec-WebSocket-Accept: {handshakeKey}\r\n\r\n";

            Send(Encoding.UTF8.GetBytes(response));

            State = WebSocketConnectionState.Connected;
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

        private void OnAuthentificated(IAsyncResult iar)
        {
            var sslStream = (SslStream)iar.AsyncState;

            sslStream.EndAuthenticateAsServer(iar);
            if (sslStream.IsAuthenticated)
            {
                sslStream.BeginRead(Buffer, 0, Buffer.Length, OnReceiveData, sslStream);
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
                var received = ((SslStream)iar.AsyncState).EndRead(iar);

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

                NetworkProtocol.ProcessParsing(this, _reader.Fill(new ReadOnlySpan<byte>(Buffer, 0, received)));
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
                _sslStream.BeginRead(Buffer, 0, Buffer.Length, OnReceiveData, _sslStream);
            }
        }
    }
}
