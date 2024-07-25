using Inertia.Logging;
using System;
using System.Net.Sockets;

namespace Inertia.Network
{
    public abstract class WebSocketServerEntity : BaseTcpServer<WebSocketConnectionEntity, WebSocketServerParameters>
    {
        protected WebSocketServerEntity(WebSocketServerParameters parameters) : base(parameters)
        {
            if (!(parameters.Protocol is WebSocketNetworkProtocol))
            {
                throw new InvalidNetworkProtocolException(typeof(WebSocketNetworkProtocol));
            }
        }

        internal override void OnAcceptConnection(IAsyncResult iar)
        {
            try
            {
                if (!IsRunning) return;

                var socket = ((Socket)iar.AsyncState).EndAccept(iar);
                var connection = new WebSocketConnectionEntity(socket, (uint)_idProvider.NextValue(), _parameters, _parameters.SslCertificate);
                
                connection.ConnectionEstablished += ConnectionEstablished;
                connection.Disconnecting += ConnectionDisconnecting;

                _connections.TryAdd(connection.Id, connection);
                connection.BeginReceiveMessages();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, GetType(), nameof(OnAcceptConnection));

                if (ex is SocketException || ex is ObjectDisposedException)
                {
                    return;
                }
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
            var connection = e.Connection as WebSocketConnectionEntity;

            _connections.TryRemove(connection.Id, out _);
            connection.Disconnecting -= ConnectionDisconnecting;

            OnConnectionDisconnecting(connection, e.Reason);
        }
    }
}