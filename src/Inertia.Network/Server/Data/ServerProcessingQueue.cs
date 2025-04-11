using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Inertia.Network
{
    public sealed class ServerProcessingQueue
    {
        private readonly BlockingCollection<Action> _queue = new BlockingCollection<Action>();
        private CancellationTokenSource? _cts;

        private int _registeredConnectionCount;
        private bool _isRunning;

        internal ServerProcessingQueue()
        {
            _queue = new BlockingCollection<Action>();           
        }

        internal int ConnectionCount => _registeredConnectionCount;

        internal void Stop()
        {
            _isRunning = false;
            _cts?.Cancel();
        }
        internal void RegisterConnection(ConnectionEntity connection)
        {
            Interlocked.Increment(ref _registeredConnectionCount);

            if (connection is TcpConnectionEntity tcpConnection)
            {
                tcpConnection.Disconnecting += ConnectionDisconnecting;
            }
        }
        internal void Enqueue(Action action)
        {
            if (!_isRunning)
            {
                StartQueueExecution();
            }

            _queue.Add(action);
        }

        private void StartQueueExecution()
        {
            _isRunning = true;
            _cts = new CancellationTokenSource();

            Task.Factory.StartNew(() => {
                try
                {
                    foreach (var action in _queue.GetConsumingEnumerable(_cts.Token))
                    {
                        action?.Invoke();
                    }
                }
                catch (OperationCanceledException oce)
                {
                    //
                }
                catch (Exception ex)
                {
                    //
                }
            }, _cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }
        private void ConnectionDisconnecting(object sender, ConnectionDisconnectingArgs e)
        {
            Interlocked.Decrement(ref _registeredConnectionCount);

            if (e.Connection is TcpConnectionEntity tcpConnection)
            {
                tcpConnection.Disconnecting -= ConnectionDisconnecting;
            }
        }
    }
}