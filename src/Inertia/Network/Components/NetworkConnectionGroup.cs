using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Inertia.Network
{
    public class NetworkConnectionGroup
    {
        public int ConnectionCount => _connections.Count;

        private event BasicAction<byte[]> Sending;

        private readonly List<NetworkConnectionEntity> _connections;
        private readonly object _locker;

        internal NetworkConnectionGroup()
        {
            _locker = new object();
            _connections = new List<NetworkConnectionEntity>();
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
    
        public void SendAsync(NetworkMessage message)
        {
            Task.Factory.StartNew(() => {
                var data = NetworkProtocol.Current.SerializeMessage(message);
                Sending?.Invoke(data);
            });
        }
    }
}