using System;
using System.Collections.Concurrent;

namespace Inertia
{
    public abstract class ActionQueueBase : IDisposable
    {
        internal const int DefaultMaxDequeuePerExecution = 60;

        public bool IsDisposed { get; private set; }
        public int Count => _queue.Count;

        private readonly int _maxExecutionPerExecution;
        private ConcurrentQueue<Action> _queue;

        protected ActionQueueBase() : this(DefaultMaxDequeuePerExecution)
        {
        }
        protected ActionQueueBase(int maximumDequeuePerExecution)
        {
            _maxExecutionPerExecution = maximumDequeuePerExecution;
            _queue = new ConcurrentQueue<Action>();

            if (_maxExecutionPerExecution <= 0)
            {
                _maxExecutionPerExecution = DefaultMaxDequeuePerExecution;
            }
        }

        public void Enqueue(Action action)
        {
            this.ThrowIfDisposable(IsDisposed);

            _queue.Enqueue(action);
        }
        public void Dispose()
        {
            Dispose(true);
        }

        protected void ProcessQueue()
        {
            if (IsDisposed || Count == 0) return;

            DoProcessQueue();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed && disposing)
            {
                _queue = null;
                IsDisposed = true;

                while (Count > 0)
                {
                    DoProcessQueue();
                }
            }
        }

        private void DoProcessQueue()
        {
            for (var i = 0; i < _maxExecutionPerExecution; i++)
            {
                if (_queue.TryDequeue(out var action))
                {
                    action?.Invoke();
                }
                else break;
            }
        }
    }
}
