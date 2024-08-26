using System;

namespace Inertia
{
    public sealed class ActionQueue : ActionQueueBase
    {
        public ActionQueue()
        {
        }
        public ActionQueue(int maximumDequeuePerExecution) : base(maximumDequeuePerExecution)
        {
        }

        public void Execute()
        {
            this.ThrowIfDisposable(IsDisposed);

            ProcessQueue();
        }
    }
}