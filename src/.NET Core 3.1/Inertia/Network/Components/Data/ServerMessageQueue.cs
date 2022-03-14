using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
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

            Task.Factory.StartNew(async () => {
                while (!IsDisposed)
                {
                    if (_queue.Count == 0)
                    {
                        await Task.Delay(10).ConfigureAwait(false);
                        continue;
                    }

                    Execute();
                }
            }, TaskCreationOptions.LongRunning);
        }

        internal void RegisterConnection(NetworkConnectionEntity connection)
        {
            connection.AssignedMessageQueue = this;
            Interlocked.Increment(ref _registeredConnection);

            if (connection is TcpConnectionEntity tcpConnection)
            {
                tcpConnection.Disconnected += (reason) =>
                {
                    Interlocked.Decrement(ref _registeredConnection);
                };
            }
        }
        internal void Enqueue(BasicAction action)
        {
            _queue.Enqueue(action);
        }
        internal void Execute()
        {
            while (_queue.TryDequeue(out BasicAction action))
            {
                action?.Invoke();
            }
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                _queue.Clear();
                _queue = null;

                IsDisposed = true;
            }
        }
    }
}
