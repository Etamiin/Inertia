using Inertia.Logging;
using System;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace Inertia.Network
{
    public abstract class WebSocketServerEntity : TcpServerBase<WebSocketConnectionEntity>
    {
        private readonly X509Certificate? _sslCertificate;

        protected WebSocketServerEntity(string ip, int port) : this(ip, port, null)
        {
        }
        protected WebSocketServerEntity(string ip, int port, X509Certificate? sslCertificate) : base(ip, port)
        {
            _sslCertificate = sslCertificate;
        }

        internal override void OnAcceptConnection(IAsyncResult iar)
        {
            try
            {
                if (!IsRunning) return;

                var socket = ((Socket)iar.AsyncState).EndAccept(iar);
                var connection = new WebSocketConnectionEntity(socket, (uint)ConnectionIdProvider.GetNextId(), _sslCertificate);
                
                connection.ConnectionEstablished += ConnectionEstablished;
                connection.Disconnecting += ConnectionDisconnecting;

                _connections.TryAdd(connection.Id, connection);
                connection.BeginReceiveMessages();
            }
            catch (Exception ex)
            {
                LoggingProvider.LogHandler.Log(LogLevel.Error, $"WebSocket connection establishment failed.", ex);

                if (ex is SocketException || ex is ObjectDisposedException) return;
            }

            base.OnAcceptConnection(iar);
        }
        
        private void ConnectionEstablished(WebSocketConnectionEntity connection)
        {
            connection.ConnectionEstablished -= ConnectionEstablished;
            OnConnectionEstablished(connection);
        }
        private void ConnectionDisconnecting(object sender, ConnectionDisconnectingArgs e)
        {
            if (e.Connection is WebSocketConnectionEntity connection && _connections.TryRemove(connection.Id, out _))
            {
                connection.Disconnecting -= ConnectionDisconnecting;

                OnConnectionDisconnecting(connection, e.Reason);
            }
        }
    }
}