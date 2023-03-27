using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Net;
using System.Linq;

namespace Inertia.Network
{
    public abstract class BaseTcpServer<T, TParameters> : NetworkServerEntity<TParameters>, IDisposable where T : TcpConnectionEntity where TParameters : ServerParameters
    {
        public bool IsDisposed { get; private set; }
        public bool IsRunning => Socket != null && Socket.IsBound;
        public int ConnectedCount => Connections.Count;

        private protected Socket Socket { get; private set; }
        private protected readonly ConcurrentDictionary<uint, T> Connections;

        protected BaseTcpServer(TParameters parameters) : base(parameters)
        {
            Connections = new ConcurrentDictionary<uint, T>();
        }

        public NetworkConnectionGroup CreateConnectionGroup()
        {
            var group = new NetworkConnectionGroup();

            group.AddConnections(Connections.Values);

            return group;
        }
        public NetworkConnectionGroup CreateConnectionGroup(Predicate<T> predicate)
        {
            var group = new NetworkConnectionGroup();
            var connections = Connections.Values
                .Where((connection) => predicate(connection));

            group.AddConnections(connections);

            return group;
        }

        public bool TryGetConnection(uint id, out T connection)
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

                    OnStarted();
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

        protected virtual void OnConnectionConnected(T connection) { }
        protected virtual void OnConnectionDisconnecting(T connection, NetworkDisconnectReason reason) { }

        internal abstract void OnAcceptConnection(IAsyncResult iar);

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
