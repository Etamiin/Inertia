using System;
using System.Net.Sockets;

namespace Inertia.Network
{
    public abstract class WebSocketServerEntity : BaseTcpServer<WebSocketConnectionEntity, WebSocketServerParameters>
    {
        protected WebSocketServerEntity(WebSocketServerParameters parameters) : base(parameters)
        {
        }

        internal override void OnAcceptConnection(IAsyncResult iar)
        {
            try
            {
                var socket = ((Socket)iar.AsyncState).EndAccept(iar);
                var connection = new WebSocketConnectionEntity(Socket, (uint)IdProvider.NextValue(), Parameters.SslCertificate);
                connection.ConnectionEstablished += ConnectionEstablished;
                connection.Disconnecting += ConnectionDisconnecting;

                Connections.TryAdd(connection.Id, connection);
                connection.BeginReceiveMessages();
            }
            catch (Exception e)
            {
                if (e is SocketException || e is ObjectDisposedException)
                {
                    return;
                }
            }

            if (IsRunning)
            {
                Socket.BeginAccept(OnAcceptConnection, Socket);
            }
        }
        private void ConnectionEstablished(WebSocketConnectionEntity connection)
        {
            connection.ConnectionEstablished -= ConnectionEstablished;
            OnConnectionConnected(connection);
        }
        private void ConnectionDisconnecting(TcpConnectionEntity connection, NetworkDisconnectReason reason)
        {
            Connections.TryRemove(connection.Id, out _);
            connection.Disconnecting -= ConnectionDisconnecting;

            OnConnectionDisconnecting((WebSocketConnectionEntity)connection, reason);
        }
    }
}
