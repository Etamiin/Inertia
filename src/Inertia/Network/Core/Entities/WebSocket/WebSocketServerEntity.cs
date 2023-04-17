using System;
using System.ComponentModel;
using System.Net.Sockets;

namespace Inertia.Network
{
    public abstract class WebSocketServerEntity : BaseTcpServer<WebSocketConnectionEntity, WebSocketServerParameters>
    {
        protected WebSocketServerEntity(WebSocketServerParameters parameters) : base(parameters)
        {
            if (parameters.Protocol == NetworkProtocolFactory.DefaultProtocol)
            {
                parameters.Protocol = NetworkProtocolFactory.DefaultWsProtocol;
            }
        }

        internal override void OnAcceptConnection(IAsyncResult iar)
        {
            try
            {
                var socket = ((Socket)iar.AsyncState).EndAccept(iar);
                var connection = new WebSocketConnectionEntity(socket, (uint)IdProvider.NextValue(), Parameters, Parameters.SslCertificate);
                connection.ConnectionEstablished += ConnectionEstablished;
                connection.Disconnecting += ConnectionDisconnecting;

                _connections.TryAdd(connection.Id, connection);
                connection.BeginReceiveMessages();
            }
            catch (Exception ex)
            {
                if (ex is SocketException || ex is ObjectDisposedException)
                {
                    return;
                }
            }

            if (IsRunning)
            {
                _socket?.BeginAccept(OnAcceptConnection, _socket);
            }
        }
        
        private void ConnectionEstablished(WebSocketConnectionEntity connection)
        {
            connection.ConnectionEstablished -= ConnectionEstablished;
            OnConnectionConnected(connection);
        }
        private void ConnectionDisconnecting(object sender, ConnectionDisconnectingArgs e)
        {
            var connection = e.Connection as WebSocketConnectionEntity;

            _connections.TryRemove(connection.Id, out _);
            connection.Disconnecting -= ConnectionDisconnecting;

            OnConnectionDisconnecting(connection, e.Reason);
        }
    }
}