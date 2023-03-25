using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Inertia
{
    public class AsyncExecutionQueuePool : IDisposable
    {
        private readonly static TimeSpan MinTimeQueueAlive= TimeSpan.FromMinutes(1);

        public bool IsDisposed { get; private set; }

        internal int QueueCapacity { get; set; }

        private SafeOrderedIntProvider _idProvider;
        private ConcurrentDictionary<int, SyncQueue> _queues;
        private SyncQueue _currentQueue;
        private object _locker;
        private TimeSpan _maxTimeQueueAlive;
        private bool _limitProcessorUsage;

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
                        queue.Disposing += Queue_Disposing;

                        _queues.TryAdd(queue.Id, queue);
                    }

                    _currentQueue = queue;
                }
            }

            _currentQueue?.Enqueue(action);
        }
        public void Dispose()
        {
            OnDispose(true);
        }

        private void OnDispose(bool disposing)
        {
            if (IsDisposed) return;

            if (disposing)
            {
                IsDisposed = true;

                SyncQueue[] queues;
                lock (_locker)
                {
                    queues = _queues.Values.ToArray();
                }

                foreach (var queue in queues)
                {
                    queue.Dispose();
                }

                _currentQueue = null;
                _idProvider = null;
            }
        }
        private void Queue_Disposing(SyncQueue queue)
        {
            _queues.TryRemove(queue.Id, out _);

            queue.Disposing -= Queue_Disposing;
        }
    }
}