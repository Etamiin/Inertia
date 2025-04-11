using Inertia.Logging;
using System;
using System.Net.Sockets;

namespace Inertia.Network
{
    public abstract class TcpServerEntity : TcpServerBase<TcpConnectionEntity>
    {        
        protected TcpServerEntity(string ip, int port) : base(ip, port)
        {
        }

        internal override void OnAcceptConnection(IAsyncResult iar)
        {
            try
            {
                if (!IsRunning) return;

                var socket = ((Socket)iar.AsyncState).EndAccept(iar);
                var connection = new TcpConnectionEntity(socket, (uint)_clientIdProvider.NextValue());

                connection.Disconnecting += ConnectionDisconnecting;

                _connections.TryAdd(connection.Id, connection);

                connection.BeginReceiveMessages();
                OnConnectionEstablished(connection);
            }
            catch (Exception ex)
            {
                LoggingProvider.LogHandler.Log(LogLevel.Error, $"An error occured when accepting TCP socket connection", ex);

                if (ex is SocketException || ex is ObjectDisposedException) return;
            }

            base.OnAcceptConnection(iar);
        }

        private void ConnectionDisconnecting(object sender, ConnectionDisconnectingArgs e)
        {
            if (e.Connection is TcpConnectionEntity connection && _connections.TryRemove(connection.Id, out _))
            {
                connection.Disconnecting -= ConnectionDisconnecting;

                OnConnectionDisconnecting(connection, e.Reason);
            }
        }
    }
}