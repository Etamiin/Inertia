namespace Inertia
{
    public sealed class SyncActionQueue : ActionQueue
    {
        public SyncActionQueue() : base()
        {
        }
        public SyncActionQueue(int maximumExecutionPerTick) : base(maximumExecutionPerTick)
        {
        }

        public void Execute()
        {
            if (DisposeRequested) return;

            ProcessQueue();
        }

        public override void RequestDispose()
        {
            base.RequestDispose();
            Clean();
        }
    }
}