using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Inertia
{
    public class ActionQueuePool : IDisposable
    {
        private readonly static TimeSpan MinTimeQueueAlive = TimeSpan.FromMinutes(5);

        public bool IsDisposed { get; private set; }

        internal int QueueCapacity { get; set; }

        private readonly ConcurrentDictionary<int, ActionQueue> _queues;
        private readonly object _locker;
        private readonly TimeSpan _maxTimeQueueAlive;
        private readonly bool _limitProcessorUsage;
        private readonly SafeOrderedIntProvider _idProvider;
        private ActionQueue? _currentQueue;
        private bool _asyncEnabled;

        public ActionQueuePool(int queueCapacity, bool limitProcessorUsage) : this(queueCapacity, limitProcessorUsage, TimeSpan.FromMinutes(5))
        {
        }
        public ActionQueuePool(int queueCapacity, bool limitProcessorUsage, TimeSpan maxTimeQueueAlive)
        {
            if (maxTimeQueueAlive < MinTimeQueueAlive)
            {
                maxTimeQueueAlive = TimeSpan.FromMinutes(5);
            }

            QueueCapacity = queueCapacity;
            _idProvider = new SafeOrderedIntProvider();
            _queues = new ConcurrentDictionary<int, ActionQueue>();
            _locker = new object();
            _maxTimeQueueAlive = maxTimeQueueAlive;
            _limitProcessorUsage = limitProcessorUsage;
            _asyncEnabled = true;
        }

        public void SyncProcessPool()
        {
            this.ThrowIfDisposable(IsDisposed);

            if (_queues.Count == 0) return;
            if (_queues.Count == 1 && _currentQueue != null)
            {
                _currentQueue.SyncProcessQueue();
            }
            else
            {
                lock (_locker)
                {
                    foreach (var pair in _queues)
                    {
                        pair.Value.SyncProcessQueue();
                    }
                }
            }
        }
        public void SetActiveAsyncExecution(bool active)
        {
            _asyncEnabled = active;
            if (_queues.Count == 1 && _currentQueue != null)
            {
                _currentQueue.SetAsyncExecutionState(active);
            }
            else
            {
                lock (_locker)
                {
                    foreach (var pair in _queues)
                    {
                        pair.Value.SetAsyncExecutionState(active);
                    }
                }
            }
        }
        public void Enqueue(BasicAction action)
        {
            this.ThrowIfDisposable(IsDisposed);

            if (_currentQueue == null || _currentQueue.Count >= QueueCapacity)
            {
                lock (_locker)
                {
                    var queue = _queues.FirstOrDefault((pair) => !pair.Value.DisposeRequested && pair.Value.Count < QueueCapacity).Value;
                    if (queue == null)
                    {
                        queue = new ActionQueue(_idProvider.NextValue(), _maxTimeQueueAlive, _limitProcessorUsage, _asyncEnabled);
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
                ActionQueue[] queues;
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
        private void QueueDisposing(ActionQueue queue)
        {
            _queues.TryRemove(queue.Id, out _);

            queue.Disposing -= QueueDisposing;
        }
    }
}