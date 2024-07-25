using Inertia.Logging;
using System;
using System.Net.Sockets;

namespace Inertia.Network
{
    public abstract class TcpServerEntity : BaseTcpServer<TcpConnectionEntity, TcpServerParameters>
    {        
        protected TcpServerEntity(TcpServerParameters parameters) : base(parameters)
        {
        }

        internal override void OnAcceptConnection(IAsyncResult iar)
        {
            try
            {
                var socket = ((Socket)iar.AsyncState).EndAccept(iar);
                var connection = new TcpConnectionEntity(socket, (uint)_idProvider.NextValue(), _parameters);

                connection.Disconnecting += ConnectionDisconnecting;

                _connections.TryAdd(connection.Id, connection);

                connection.BeginReceiveMessages();
                OnConnectionEstablished(connection);
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

        private void ConnectionDisconnecting(object sender, ConnectionDisconnectingArgs e)
        {
            var connection = e.Connection as TcpConnectionEntity;

            _connections.TryRemove(connection.Id, out _);
            connection.Disconnecting -= ConnectionDisconnecting;

            OnConnectionDisconnecting(connection, e.Reason);
        }
    }
}