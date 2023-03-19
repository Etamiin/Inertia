using Inertia.Scriptable;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Inertia
{
    internal class SyncQueue
    {
        internal event BasicAction<SyncQueue> Disposing;

        internal int Id { get; private set; }
        internal DateTime? EmptySince { get; private set; }
        internal bool DisposeRequested { get; private set; }
        internal int Count
        {
            get
            {
                if (_queue == null) return -1;

                return _queue.Count;
            }
        }

        private ConcurrentQueue<BasicAction> _queue;
        private bool _isRunning;
        private TimeSpan _maxTimeQueueAlive;

        internal SyncQueue(int id, TimeSpan maxTimeQueueAlive)
        {
            Id = id;
            _maxTimeQueueAlive = maxTimeQueueAlive;
            _queue = new ConcurrentQueue<BasicAction>();

            Task.Factory.StartNew(Running, TaskCreationOptions.LongRunning);
        }

        internal void Enqueue(BasicAction action)
        {
            _queue.Enqueue(action);
        }
        internal void Dispose()
        {
            DisposeRequested = true;
        }

        private async void Running()
        {
            _isRunning = true;

            while (_isRunning)
            {
                if (_queue.Count == 0)
                {
                    if (DisposeRequested)
                    {
                        Disposing?.Invoke(this);

                        EmptySince = null;
                        _queue?.Clear();
                        _queue = null;
                        _isRunning = false;
                    }
                    else
                    {
                        if (EmptySince.HasValue)
                        {
                            var span = DateTime.Now - EmptySince.Value;
                            if (span >= _maxTimeQueueAlive) Dispose();
                        }
                        else
                        {
                            EmptySince = DateTime.Now;
                        }

                        await Task.Delay(10).ConfigureAwait(false);
                    }
                }
                else
                {
                    EmptySince = null;

                    if (Run.LimitProcessorUsage)
                    {
                        while (_queue.TryDequeue(out var action))
                        {
                            action?.Invoke();
                        }

                        await Task.Delay(10).ConfigureAwait(false);
                    }
                    else if (_queue.TryDequeue(out var action)) action?.Invoke();
                }
            }
        }
    }
}
