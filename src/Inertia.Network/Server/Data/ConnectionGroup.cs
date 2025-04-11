using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Inertia.Network
{
    public class ConnectionGroup
    {
        private event BytesSenderAction? SendingData;

        private readonly NetworkProtocol _networkProtocol;
        private readonly List<ConnectionEntity> _connections;
        private readonly object _locker;

        public ConnectionGroup(NetworkProtocol networkProtocol)
        {
            _networkProtocol = networkProtocol;
            _connections = new List<ConnectionEntity>();
            _locker = new object();
        }
        
        public int ConnectionCount => _connections.Count;

        public void AddConnection(ConnectionEntity connection)
        {
            lock (_locker)
            {
                SendingData += connection.Send;
                _connections.Add(connection);
            }
        }
        public void AddConnections(IEnumerable<ConnectionEntity> connections)
        {
            lock (_locker)
            {
                foreach (var connection in connections)
                {
                    SendingData += connection.Send;
                    _connections.Add(connection);
                }
            }
        }
        public void RemoveConnection(ConnectionEntity connection)
        {
            lock (_locker)
            {
                SendingData -= connection.Send;
                _connections.Remove(connection);
            }
        }
        public void RemoveConnections(Predicate<ConnectionEntity> predicate)
        {
            lock (_locker)
            {
                var i = 0;
                while (i < _connections.Count)
                {
                    var connection = _connections[i];
                    if (predicate(connection))
                    {
                        SendingData -= connection.Send;
                        _connections.RemoveAt(i);

                        continue;
                    }

                    i++;
                }
            }
        }
    
        public Task SendAsync(byte[] data)
        {
            if (SendingData is null) return Task.CompletedTask;

            return Task.Run(() => SendingData?.Invoke(data));
        }
        public Task SendAsync(NetworkMessage message)
        {
            if (SendingData is null) return Task.CompletedTask;

            return Task.Run(() =>
            {
                SendingData?.Invoke(_networkProtocol.SerializeMessage(message));
            });
        }
    }
}