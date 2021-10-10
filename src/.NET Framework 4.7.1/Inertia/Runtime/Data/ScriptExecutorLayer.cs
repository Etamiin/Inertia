namespace Inertia.Runtime
{
    internal sealed class ScriptExecutorLayer
    {
        internal event BasicAction Executor = () => { };

        internal bool IsDisposed { get; private set; }

        internal void Join(Script script)
        {
            if (IsDisposed)
                return;

            script.AttachedLayer = this;

            Executor += script.Update;
        }
        internal void Leave(Script script)
        {
            if (IsDisposed)
                return;

            script.AttachedLayer = null;

            Executor -= script.Update;
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
