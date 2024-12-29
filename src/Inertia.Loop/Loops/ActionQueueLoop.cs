using System;
using System.Collections.Concurrent;

namespace Inertia.Loop
{
    public class ActionQueueLoop : LoopBase
    {
        private ConcurrentQueue<Action> _queue;

        public ActionQueueLoop() : this(LoopMode.AlwaysSleep, new LoopTime())
        {
        }
        public ActionQueueLoop(LoopMode loopMode) : this(loopMode, new LoopTime())
        {
        }
        public ActionQueueLoop(LoopTime loopTime) : this(LoopMode.AlwaysSleep, loopTime)
        {
        }
        public ActionQueueLoop(LoopMode loopMode, LoopTime loopTime) : base(loopMode, loopTime)
        {
            _queue = new ConcurrentQueue<Action>();
        }

        public void Enqueue(Action action)
        {
            this.ThrowIfDisposable(IsDisposed);

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            _queue.Enqueue(action);
        }

        protected override void DoRun(LoopTime loopTime)
        {
            this.ThrowIfDisposable(IsDisposed);

            if (_queue == null) return;

            while (_queue.TryDequeue(out var action))
            {
                action();
            }
        }
        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed && disposing)
            {
                _queue.Clear();
            }

            base.Dispose(disposing);
        }
    }
}