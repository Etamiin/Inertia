using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Inertia.Network
{
    public class ConnectionGroup
    {
        private BytesSenderAction? _sendingData;

        private readonly NetworkProtocol _networkProtocol;
        private readonly HashSet<TcpConnectionEntity> _connections;
        private readonly object _locker;

        public ConnectionGroup(NetworkProtocol networkProtocol)
        {
            _networkProtocol = networkProtocol;
            _connections = new HashSet<TcpConnectionEntity>();
            _locker = new object();
        }
        
        public int ConnectionCount => _connections.Count;

        public void AddConnection(TcpConnectionEntity connection)
        {
            lock (_locker)
            {
                _sendingData += connection.Send;
                _connections.Add(connection);
            }
        }
        public void AddConnections(IEnumerable<TcpConnectionEntity> connections)
        {
            lock (_locker)
            {
                foreach (var connection in connections)
                {
                    _sendingData += connection.Send;
                    _connections.Add(connection);
                }
            }
        }
        public void RemoveConnection(TcpConnectionEntity connection)
        {
            lock (_locker)
            {
                _sendingData -= connection.Send;
                _connections.Remove(connection);
            }
        }
        public void RemoveConnections(Predicate<TcpConnectionEntity> predicate)
        {
            lock (_locker)
            {
                _connections.RemoveWhere(predicate);
            }
        }
        public void Clear()
        {
            lock (_locker)
            {
                foreach (var conn in _connections)
                {
                    _sendingData -= conn.Send;
                }

                _connections.Clear();
            }
        }

        public Task SendAsync(byte[] data)
        {
            if (_sendingData is null) return Task.CompletedTask;

            return Task.Run(() => _sendingData?.Invoke(data));
        }
        public Task SendAsync(NetworkMessage message)
        {
            if (_sendingData is null) return Task.CompletedTask;

            return Task.Run(() =>
            {
                _sendingData?.Invoke(_networkProtocol.SerializeMessage(message));
            });
        }
    }
}