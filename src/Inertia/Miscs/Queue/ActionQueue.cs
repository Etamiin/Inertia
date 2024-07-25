namespace Inertia
{
    public sealed class ActionQueue : ActionQueueBase
    {
        public ActionQueue()
        {
        }
        public ActionQueue(int maximumExecutionPerTick) : base(maximumExecutionPerTick)
        {
        }

        public void Execute()
        {
            if (_isDisposing || Count == 0) return;

            ProcessQueue();
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed && disposing)
            {
                Clean();
            }
        }
    }
}