using System;
using System.Collections.Concurrent;

namespace Inertia
{
    public abstract class ActionQueue
    {
        private protected bool DisposeRequested { get; private set; }

        public bool IsDisposed { get; private set; }
        public int Count => _queue.Count;

        private protected readonly ActionQueueParameters _parameters;
        private ConcurrentQueue<BasicAction> _queue;

        protected ActionQueue(ActionQueueParameters parameters)
        {
            _parameters = parameters;
            _queue = new ConcurrentQueue<BasicAction>();
        }

        public void Enqueue(BasicAction action)
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
            while (i < _parameters.MaximumExecutionPerTick);
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
