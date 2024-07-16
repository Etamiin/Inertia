using System;
using System.Collections.Concurrent;

namespace Inertia
{
    public abstract class ActionQueueBase : IDisposable
    {
        internal const int DefaultMaxExecutionPerTick = 60;

        public bool IsDisposed { get; private set; }
        public int Count => _queue.Count;

        private int _maxExecutionPerTick;
        private ConcurrentQueue<Action> _queue;
        private protected bool _isDisposing;

        protected ActionQueueBase() : this(DefaultMaxExecutionPerTick)
        {
        }
        protected ActionQueueBase(int maximumExecutionPerTick)
        {
            _maxExecutionPerTick = maximumExecutionPerTick;
            _queue = new ConcurrentQueue<Action>();

            if (_maxExecutionPerTick <= 0)
            {
                _maxExecutionPerTick = DefaultMaxExecutionPerTick;
            }
        }

        public void Enqueue(Action action)
        {
            if (IsDisposed || _isDisposing) return;

            _queue.Enqueue(action);
        }
        public void Dispose()
        {
            Dispose(true);
        }

        protected void ProcessQueue()
        {
            if (IsDisposed || Count == 0) return;

            for (var i = 0; i < _maxExecutionPerTick; i++)
            {
                if (_queue.TryDequeue(out var action))
                {
                    action?.Invoke();
                }
                else break;
            }
        }
        protected void Clean()
        {
            if (IsDisposed) return;

            while (Count > 0)
            {
                ProcessQueue();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed && disposing)
            {
                _isDisposing = true;
                _queue = null;
                IsDisposed = true;
                _isDisposing = false;
            }
        }
    }
}
