using System;
using System.Net.Sockets;

namespace Inertia.Network
{
    public abstract class TcpServerEntity : BaseTcpServer<TcpConnectionEntity, ServerParameters>
    {        
        protected TcpServerEntity(ServerParameters parameters) : base(parameters)
        {
        }

        internal protected override void OnAcceptConnection(IAsyncResult iar)
        {
            try
            {
                var socket = ((Socket)iar.AsyncState).EndAccept(iar);
                var connection = new TcpConnectionEntity(socket, (uint)IdProvider.NextValue());

                _connections.TryAdd(connection.Id, connection);

                connection.BeginReceiveMessages();
                OnConnectionConnected(connection);
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
                _socket.BeginAccept(OnAcceptConnection, _socket);
            }
        }
        private void ConnectionDisconnecting(TcpConnectionEntity connection, NetworkDisconnectReason reason)
        {
            _connections.TryRemove(connection.Id, out _);
            connection.Disconnecting -= ConnectionDisconnecting;

            OnConnectionDisconnecting(connection, reason);
        }
    }
}
