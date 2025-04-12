using Inertia.Logging;
using System;
using System.Collections.Concurrent;
using System.Diagnostics.SymbolStore;
using System.Threading;
using System.Threading.Tasks;

namespace Inertia.Network
{
    internal sealed class ServerProcessingQueue
    {
        private readonly BlockingCollection<Action> _queue;

        private int _registeredConnectionCount;
        private int _isStarted;

        internal ServerProcessingQueue()
        {
            _queue = new BlockingCollection<Action>();           
        }

        public int ConnectionCount => _registeredConnectionCount;

        internal void RegisterConnection(TcpConnectionEntity connection)
        {
            Interlocked.Increment(ref _registeredConnectionCount);

            connection.Disconnecting += ConnectionDisconnecting;
        }
        internal void Enqueue(Action action)
        {
            if (Interlocked.CompareExchange(ref _isStarted, 1, 0) == 0)
            {
                StartQueueExecution();
            }

            _queue.Add(action);
        }

        private void StartQueueExecution()
        {
            Task.Factory.StartNew(() => {
                foreach (var action in _queue.GetConsumingEnumerable())
                {
                    try
                    {
                        action?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        LoggingProvider.LogHandler.Log(LogLevel.Error, $"An error occurred when executing an action in the server processing queue", ex);
                    }
                }
            }, TaskCreationOptions.LongRunning);
        }
        private void ConnectionDisconnecting(object sender, ConnectionDisconnectingArgs e)
        {
            Interlocked.Decrement(ref _registeredConnectionCount);

            e.Connection.Disconnecting -= ConnectionDisconnecting;
        }
    }
}