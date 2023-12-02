namespace Inertia
{
    public sealed class SyncTickedQueue : TickedQueue
    {
        public SyncTickedQueue() : base()
        {
        }
        public SyncTickedQueue(int maximumExecutionPerTick) : base(maximumExecutionPerTick)
        {
        }

        public void Execute()
        {
            if (DisposeRequested || Count == 0) return;

            ProcessQueue();
        }

        public override void RequestDispose()
        {
            base.RequestDispose();
            Clean();
        }
    }
}