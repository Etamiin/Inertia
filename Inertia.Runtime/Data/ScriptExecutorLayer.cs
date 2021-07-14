namespace Inertia.Runtime
{
    internal sealed class ScriptExecutorLayer
    {
        public event BasicAction Executor = () => { };

        internal int Count { get; private set; }
        internal bool IsDisposed { get; private set; }
        internal bool LimitAchieved
        {
            get
            {
                return Count >= byte.MaxValue;
            }
        }

        internal void Join(Script script)
        {
            if (IsDisposed)
                return;

            script.AttachedLayer = this;

            Executor += script.Update;
            Count++;
        }
        internal void Leave(Script script)
        {
            if (IsDisposed)
                return;

            script.AttachedLayer = null;

            Executor -= script.Update;
            Count--;
        }

        internal void Execute()
        {
            if (IsDisposed)
                return;

            lock (Executor)
                Executor?.Invoke();
        }

        internal void Dispose()
        {
            if (IsDisposed)
                return;

            Executor = null;

            IsDisposed = true;
        }
    }
}
