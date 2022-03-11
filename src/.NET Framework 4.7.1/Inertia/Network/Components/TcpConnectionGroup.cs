using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Network
{
    public class NetworkConnectionGroup
    {
        public int ConnectionCount => _connections.Count;

        private event BasicAction<NetworkMessage> Sender = (msg) => { };

        private List<NetworkConnectionEntity> _connections;
        private object _locker;

        public NetworkConnectionGroup()
        {
            _locker = new object();
            _connections = new List<NetworkConnectionEntity>();
        }

        public void AddConnection(NetworkConnectionEntity connection)
        {
            lock (_locker)
            {
                Sender += connection.Send;
                _connections.Add(connection);
            }
        }
        public void AddConnections(IEnumerable<NetworkConnectionEntity> connections)
        {
            lock (_locker)
            {
                foreach (var c in connections)
                {
                    Sender += c.Send;
                    _connections.Add(c);
                }
            }
        }
        public void RemoveConnection(NetworkConnectionEntity connection)
        {
            lock (_locker)
            {
                Sender -= connection.Send;
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
                    var c = _connections[i];
                    if (predicate(c))
                    {
                        Sender -= c.Send;
                        _connections.RemoveAt(i);
                        continue;
                    }

                    i++;
                }
            }
        }
    
        public void SendAsync(NetworkMessage message)
        {
            Task.Run(() => Sender(message));
        }
    }
}