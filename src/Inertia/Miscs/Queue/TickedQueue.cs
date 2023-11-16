using System;
using System.Collections.Concurrent;

namespace Inertia
{
    public abstract class TickedQueue
    {
        internal const int DefaultMaxExecutionPerTick = 60;

        private protected bool DisposeRequested { get; private set; }

        public bool IsDisposed { get; private set; }
        public int Count => _queue.Count;

        private int _maxExecutionPerTick;
        private ConcurrentQueue<Action> _queue;

        protected TickedQueue() : this(DefaultMaxExecutionPerTick)
        {
        }
        protected TickedQueue(int maximumExecutionPerTick)
        {
            _maxExecutionPerTick = maximumExecutionPerTick;
            _queue = new ConcurrentQueue<Action>();
        }

        public void Enqueue(Action action)
        {
            if (IsDisposed || DisposeRequested) return;

            _queue.Enqueue(action);
        }

        public virtual void RequestDispose()
        {
            if (IsDisposed || DisposeRequested) return;

            DisposeRequested = true;
        }

        protected void ProcessQueue()
        {
            if (IsDisposed) return;

            var i = 0;
            do
            {
                if (_queue.TryDequeue(out var action))
                {
                    action?.Invoke();
                    i++;
                }
                else break;
            }
            while (i < _maxExecutionPerTick);
        }
        protected void Clean()
        {
            if (IsDisposed) return;

            while (Count > 0)
            {
                ProcessQueue();
            }

            _queue = null;
            IsDisposed = true;
            DisposeRequested = false;
        }
    }
}
