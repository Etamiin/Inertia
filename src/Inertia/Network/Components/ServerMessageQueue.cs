using Inertia.Logging;
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

        private ConcurrentQueue<Action> _queue;
        private ILogger _logger;
        private int _registeredConnection;

        internal ServerMessageQueue(ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            _queue = new ConcurrentQueue<Action>();
            _logger = logger;

            StartQueueExecution();            
        }

        internal void RegisterConnection(NetworkConnectionEntity connection)
        {
            Interlocked.Increment(ref _registeredConnection);

            if (connection is TcpConnectionEntity tcpConnection)
            {
                tcpConnection.Disconnecting += ConnectionDisconnecting;
            }
        }
        internal void Enqueue(Action action)
        {
            _queue.Enqueue(action);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void StartQueueExecution()
        {
            Task.Factory.StartNew(async () => {
                while (!IsDisposed)
                {
                    if (_queue.Count == 0)
                    {
                        await Task.Delay(1).ConfigureAwait(false);
                        continue;
                    }

                    try
                    {
                        while (_queue.TryDequeue(out Action action))
                        {
                            action?.Invoke();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Error on ServerMessageQueue: {ex}");
                    }
                }
            }, TaskCreationOptions.LongRunning);
        }
        private void ConnectionDisconnecting(object sender, ConnectionDisconnectingArgs e)
        {
            Interlocked.Decrement(ref _registeredConnection);

            if (e.Connection is TcpConnectionEntity tcpConnection)
            {
                tcpConnection.Disconnecting -= ConnectionDisconnecting;
            }
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
    }
}