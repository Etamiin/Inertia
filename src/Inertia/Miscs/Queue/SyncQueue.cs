using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Inertia
{
    internal class SyncQueue
    {
        private const int LimitProcessorUsageMaxDequeuePerTick = 20;

        internal event BasicAction<SyncQueue> Disposing;

        internal int Id { get; private set; }
        internal DateTime? EmptySince { get; private set; }
        internal bool DisposeRequested { get; private set; }
        internal int Count
        {
            get
            {
                return _queue.Count;
            }
        }

        private readonly ConcurrentQueue<BasicAction> _queue;
        private readonly TimeSpan _maxTimeQueueAlive;
        private readonly bool _limitProcessorUsage;

        internal SyncQueue(int id, TimeSpan maxTimeQueueAlive, bool limitProcessorUsage)
        {
            Id = id;
            _queue = new ConcurrentQueue<BasicAction>();
            _maxTimeQueueAlive = maxTimeQueueAlive;
            _limitProcessorUsage = limitProcessorUsage;

            Task.Factory.StartNew(ProcessQueue, TaskCreationOptions.LongRunning);
        }

        internal void Enqueue(BasicAction action)
        {
            _queue.Enqueue(action);
        }
        internal void BeginDispose()
        {
            DisposeRequested = true;
        }

        private async void ProcessQueue()
        {
            var isRunning = true;
            while (isRunning)
            {
                if (_queue.Count == 0)
                {
                    if (DisposeRequested)
                    {
                        Disposing?.Invoke(this);

                        EmptySince = null;
                        _queue?.Clear();
                        isRunning = false;
                    }
                    else
                    {
                        if (EmptySince.HasValue)
                        {
                            var span = DateTime.Now - EmptySince.Value;
                            if (span >= _maxTimeQueueAlive) BeginDispose();
                        }
                        else
                        {
                            EmptySince = DateTime.Now;
                        }

                        await Task.Delay(10).ConfigureAwait(false);
                    }

                    continue;
                }

                EmptySince = null;

                if (_limitProcessorUsage)
                {
                    var i = 0;
                    while (i < LimitProcessorUsageMaxDequeuePerTick && _queue.TryDequeue(out var action))
                    {
                        action?.Invoke();
                        i++;
                    }

                    await Task.Delay(10).ConfigureAwait(false);
                }
                else if (_queue.TryDequeue(out var action)) action?.Invoke();
            }
        }
    }
}
