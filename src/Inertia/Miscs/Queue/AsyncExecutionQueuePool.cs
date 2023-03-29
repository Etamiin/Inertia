using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Inertia
{
    public class AsyncExecutionQueuePool : IDisposable
    {
        private readonly static TimeSpan MinTimeQueueAlive = TimeSpan.FromMinutes(5);

        public bool IsDisposed { get; private set; }

        internal int QueueCapacity { get; set; }

        private readonly ConcurrentDictionary<int, SyncQueue> _queues;
        private readonly object _locker;
        private readonly TimeSpan _maxTimeQueueAlive;
        private readonly bool _limitProcessorUsage;
        private SafeOrderedIntProvider _idProvider;
        private SyncQueue? _currentQueue;

        public AsyncExecutionQueuePool(int queueCapacity, bool limitProcessorUsage) : this(queueCapacity, limitProcessorUsage, TimeSpan.FromMinutes(5))
        {
        }
        public AsyncExecutionQueuePool(int queueCapacity, bool limitProcessorUsage, TimeSpan maxTimeQueueAlive)
        {
            if (maxTimeQueueAlive < MinTimeQueueAlive)
            {
                maxTimeQueueAlive = TimeSpan.FromMinutes(5);
            }

            QueueCapacity = queueCapacity;
            _idProvider = new SafeOrderedIntProvider();
            _queues = new ConcurrentDictionary<int, SyncQueue>();
            _locker = new object();
            _maxTimeQueueAlive = maxTimeQueueAlive;
            _limitProcessorUsage = limitProcessorUsage;
        }

        public void Enqueue(BasicAction action)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(AsyncExecutionQueuePool));
            }

            if (_currentQueue == null || _currentQueue.Count >= QueueCapacity)
            {
                lock (_locker)
                {
                    var queue = _queues.FirstOrDefault((pair) => !pair.Value.DisposeRequested && pair.Value.Count < QueueCapacity).Value;
                    if (queue == null)
                    {
                        queue = new SyncQueue(_idProvider.NextValue(), _maxTimeQueueAlive, _limitProcessorUsage);
                        queue.Disposing += QueueDisposing;

                        _queues.TryAdd(queue.Id, queue);
                    }

                    _currentQueue = queue;
                }
            }

            _currentQueue?.Enqueue(action);
        }
        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (IsDisposed) return;

            IsDisposed = true;

            if (disposing)
            {
                SyncQueue[] queues;
                lock (_locker)
                {
                    queues = _queues.Values.ToArray();
                }

                foreach (var queue in queues)
                {
                    queue.BeginDispose();
                }

                _currentQueue = null;
            }
        }
        private void QueueDisposing(SyncQueue queue)
        {
            _queues.TryRemove(queue.Id, out _);

            queue.Disposing -= QueueDisposing;
        }
    }
}