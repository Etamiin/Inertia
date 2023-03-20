using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Inertia.Network
{
    internal class ServerMessageQueue : IDisposable
    {
        internal bool IsDisposed { get; private set; }
        internal int ConnectionCount => _registeredConnection;

        private ConcurrentQueue<BasicAction> _queue;
        private int _registeredConnection;

        internal ServerMessageQueue()
        {
            _queue = new ConcurrentQueue<BasicAction>();
            StartAutoExecuteAsync();            
        }

        internal void RegisterConnection(NetworkConnectionEntity connection)
        {
            Interlocked.Increment(ref _registeredConnection);

            if (connection is TcpConnectionEntity tcpConnection)
            {
                tcpConnection.Disconnecting += TcpConnection_Disconnected;
            }
            //check for udp connection
        }
        internal void Enqueue(BasicAction action)
        {
            _queue.Enqueue(action);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (IsDisposed) return;

            if (disposing)
            {
                _queue.Clear();
                _queue = null;
            }

            IsDisposed = true;
        }
    
        private void StartAutoExecuteAsync()
        {
            Task.Factory.StartNew(async () => {
                while (!IsDisposed)
                {
                    if (_queue.Count == 0)
                    {
                        await Task.Delay(10).ConfigureAwait(false);
                        continue;
                    }

                    while (_queue.TryDequeue(out BasicAction action))
                    {
                        action?.Invoke();
                    }
                }
            }, TaskCreationOptions.LongRunning);
        }
        private void TcpConnection_Disconnected(NetworkConnectionEntity entity, NetworkDisconnectReason value)
        {
            Interlocked.Decrement(ref _registeredConnection);

            if (entity is TcpConnectionEntity tcpConnection)
            {
                tcpConnection.Disconnecting += TcpConnection_Disconnected;
            }
        }
    }
}
