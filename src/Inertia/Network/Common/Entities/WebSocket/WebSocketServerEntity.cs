using System;
using System.Net.Sockets;

namespace Inertia.Network
{
    public abstract class WebSocketServerEntity : TcpServerEntity, IDisposable
    {
        public readonly new WebSocketServerParameters Parameters;

        protected WebSocketServerEntity(WebSocketServerParameters parameters) : base(parameters)
        {
            Parameters = parameters;
        }

        protected internal override TcpConnectionEntity CreateConnection(Socket socket, uint id)
        {
            return new WebSocketConnectionEntity(socket, id, Parameters.SslCertificate);
        }

        protected internal override void OnConnectionCreated(TcpConnectionEntity connection)
        {
            connection.DeterminedAsWebSocket += ConnectionWebSocketDetermined;
            connection.BeginReceiveMessages();
        }

        private void ConnectionWebSocketDetermined(TcpConnectionEntity connection)
        {
            connection.DeterminedAsWebSocket -= ConnectionWebSocketDetermined;
            OnConnectionConnected(connection);
        }
    }
}
