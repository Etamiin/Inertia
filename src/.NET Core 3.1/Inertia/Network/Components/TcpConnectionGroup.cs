using System;
using System.Collections.Generic;
using System.Text;

namespace Inertia.Network
{
    public class TcpConnectionGroup
    {
        private event BasicAction<NetworkMessage> Sender = (msg) => { };

        private List<TcpConnectionEntity> _connections;
        private object _locker;

        public TcpConnectionGroup()
        {
            _locker = new object();
            _connections = new List<TcpConnectionEntity>();
        }

        public void AddConnection(TcpConnectionEntity connection)
        {
            lock (_locker)
            {
                Sender += connection.Send;
                _connections.Add(connection);
            }
        }
        public void AddConnections(IEnumerable<TcpConnectionEntity> connections)
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
        public void RemoveConnection(TcpConnectionEntity connection)
        {
            lock (_locker)
            {
                Sender -= connection.Send;
                _connections.Remove(connection);
            }
        }
        public void RemoveConnections(Predicate<TcpConnectionEntity> predicate)
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
    
        public void Send(NetworkMessage message)
        {
            Sender(message);
        }
    }
}