using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Inertia.Network
{
    public class NetworkConnectionGroup
    {
        public int ConnectionCount => _connections.Count;

        private event BytesSenderAction? Sending;

        private readonly List<NetworkConnectionEntity> _connections;
        private readonly object _locker;

        public NetworkConnectionGroup()
        {
            _connections = new List<NetworkConnectionEntity>();
            _locker = new object();
        }

        public void AddConnection(NetworkConnectionEntity connection)
        {
            lock (_locker)
            {
                Sending += connection.Send;
                _connections.Add(connection);
            }
        }
        public void AddConnections(IEnumerable<NetworkConnectionEntity> connections)
        {
            lock (_locker)
            {
                foreach (var connection in connections)
                {
                    Sending += connection.Send;
                    _connections.Add(connection);
                }
            }
        }
        public void RemoveConnection(NetworkConnectionEntity connection)
        {
            lock (_locker)
            {
                Sending -= connection.Send;
                _connections.Remove(connection);
            }
        }
        public void RemoveConnections(Predicate<NetworkConnectionEntity> predicate)
        {
            lock (_locker)
            {
                var i = 0;
                while (i < _connections.Count)
                {
                    var connection = _connections[i];
                    if (predicate(connection))
                    {
                        Sending -= connection.Send;
                        _connections.RemoveAt(i);

                        continue;
                    }

                    i++;
                }
            }
        }
    
        public Task SendAsync(NetworkMessage message)
        {
            if (Sending == null) return Task.CompletedTask;

            return Task.Run(() =>
            {
                Sending?.Invoke(NetworkProtocolManager.CurrentProtocol.SerializeMessage(message));
            });
        }
    }
}