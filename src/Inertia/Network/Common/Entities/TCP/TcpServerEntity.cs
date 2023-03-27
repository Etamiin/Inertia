using Inertia.Logging;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Inertia.Network
{
    public abstract class TcpServerEntity : NetworkServerEntity, IDisposable
    {
        public bool IsDisposed { get; private set; }
        public bool IsRunning => Socket != null && Socket.IsBound;
        public int ConnectedCount => Connections.Count;

        internal protected Socket Socket { get; private set; }
        internal protected readonly ConcurrentDictionary<uint, TcpConnectionEntity> Connections;
        
        protected TcpServerEntity(ServerParameters parameters) : base(parameters)
        {
            Connections = new ConcurrentDictionary<uint, TcpConnectionEntity>();
        }

        public NetworkConnectionGroup CreateConnectionGroup()
        {
            var group = new NetworkConnectionGroup();

            group.AddConnections(Connections.Values);

            return group;
        }
        public NetworkConnectionGroup CreateConnectionGroup(Predicate<TcpConnectionEntity> predicate)
        {
            var group = new NetworkConnectionGroup();
            var connections = Connections.Values
                .Where((connection) => predicate(connection));

            group.AddConnections(connections);

            return group;
        }
        
        public bool TryGetConnection(uint id, out TcpConnectionEntity connection)
        {
            return Connections.TryGetValue(id, out connection);
        }

        public sealed override void Start()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(TcpServerEntity));
            }

            if (!IsRunning)
            {
                try
                {
                    Connections.Clear();

                    Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    Socket.Bind(new IPEndPoint(string.IsNullOrEmpty(Parameters.Ip) ? IPAddress.Any : IPAddress.Parse(Parameters.Ip), Parameters.Port));
                    Socket.Listen(1000);

                    Started();
                    Socket.BeginAccept(new AsyncCallback(OnAcceptConnection), Socket);
                }
                catch (Exception ex)
                {
                    Logger?.Error(ex);
                    Close(NetworkDisconnectReason.ConnectionFailed);
                }
            }
        }
        public sealed override void Close(NetworkDisconnectReason reason)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(TcpServerEntity));
            }

            if (Connections != null && Connections.Count > 0)
            {
                var entities = Connections.Values.ToArray();
                foreach (var connection in entities)
                {
                    connection.Dispose();
                }
            }

            if (IsRunning)
            {
                Socket?.Close();
                Socket = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void OnConnectionConnected(TcpConnectionEntity connection) { }
        protected virtual void OnConnectionDisconnecting(TcpConnectionEntity connection, NetworkDisconnectReason reason) { }

        internal protected virtual TcpConnectionEntity CreateConnection(Socket socket, uint id)
        {
            return new TcpConnectionEntity(socket, id);

            connection.BeginReceiveMessages();
            OnConnectionConnected(connection);
        }

        private void OnAcceptConnection(IAsyncResult iar)
        {
            try
            {
                var socket = ((Socket)iar.AsyncState).EndAccept(iar);
                var connection = CreateConnection(socket, (uint)IdProvider.NextValue());

                connection.Disconnecting += ConnectionDisconnecting;

                Connections.TryAdd(connection.Id, connection);
                OnConnectionCreated(connection);
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

        private void ConnectionDisconnecting(TcpConnectionEntity connection, NetworkDisconnectReason reason)
        {
            Connections.TryRemove(connection.Id, out _);
            connection.Disconnecting -= ConnectionDisconnecting;

            OnConnectionDisconnecting(connection, reason);
        }
        private void Dispose(bool disposing)
        {
            if (IsDisposed) return;

            if (disposing)
            {
                Close();
            }

            IsDisposed = true;
        }
    }
}
