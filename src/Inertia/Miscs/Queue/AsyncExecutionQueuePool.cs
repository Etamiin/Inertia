using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Inertia
{
    public class AsyncExecutionQueuePool : IDisposable
    {
        public bool IsDisposed { get; private set; }

        private SafeOrderedIntProvider _idProvider;
        private ConcurrentDictionary<int, SyncQueue> _queues;
        private SyncQueue _currentQueue;
        private object _locker;
        private int _queueCapacity;
        private TimeSpan _maxTimeQueueAlive;

        public AsyncExecutionQueuePool(int queueCapacity) : this(queueCapacity, TimeSpan.FromMinutes(5))
        {
        }
        public AsyncExecutionQueuePool(int queueCapacity, TimeSpan maxTimeQueueAlive)
        {
            if (maxTimeQueueAlive < TimeSpan.FromMinutes(1))
            {
                maxTimeQueueAlive = TimeSpan.FromMinutes(5);
            }

            _idProvider = new SafeOrderedIntProvider();
            _queues = new ConcurrentDictionary<int, SyncQueue>();
            _queueCapacity = queueCapacity;
            _locker = new object();
            _maxTimeQueueAlive = maxTimeQueueAlive;
        }

        public void Enqueue(BasicAction action)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(AsyncExecutionQueuePool));
            }

            if (_currentQueue == null || _currentQueue.Count >= _queueCapacity)
            {
                lock (_locker)
                {
                    var queue = _queues.FirstOrDefault((pair) => !pair.Value.DisposeRequested && pair.Value.Count < _queueCapacity).Value;
                    if (queue == null)
                    {
                        queue = new SyncQueue(_idProvider.NextInt(), _maxTimeQueueAlive);
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