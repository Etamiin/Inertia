﻿using System;
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
                _socket?.BeginAccept(OnAcceptConnection, _socket);
            }
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