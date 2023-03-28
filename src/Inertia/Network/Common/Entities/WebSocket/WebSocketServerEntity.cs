using System;
using System.Net.Sockets;

namespace Inertia.Network
{
    public abstract class WebSocketServerEntity : BaseTcpServer<WebSocketConnectionEntity, WebSocketServerParameters>
    {
        protected WebSocketServerEntity(WebSocketServerParameters parameters) : base(parameters)
        {
        }

        internal protected override void OnAcceptConnection(IAsyncResult iar)
        {
            try
            {
                var socket = ((Socket)iar.AsyncState).EndAccept(iar);
                var connection = new WebSocketConnectionEntity(socket, (uint)IdProvider.NextValue(), Parameters.SslCertificate);
                connection.ConnectionEstablished += ConnectionEstablished;
                connection.Disconnecting += ConnectionDisconnecting;

                _connections.TryAdd(connection.Id, connection);
                connection.BeginReceiveMessages();
            }
            catch (Exception e)
            {
                Logger.Error(e);
                if (e is SocketException || e is ObjectDisposedException)
                {
                    return;
                }
            }

            if (IsRunning)
            {
                _socket.BeginAccept(OnAcceptConnection, _socket);
            }
        }
        
        private void ConnectionEstablished(WebSocketConnectionEntity connection)
        {
            connection.ConnectionEstablished -= ConnectionEstablished;
            OnConnectionConnected(connection);
        }
        private void ConnectionDisconnecting(TcpConnectionEntity connection, NetworkDisconnectReason reason)
        {
            _connections.TryRemove(connection.Id, out _);
            connection.Disconnecting -= ConnectionDisconnecting;

            OnConnectionDisconnecting((WebSocketConnectionEntity)connection, reason);
        }
    }
}
