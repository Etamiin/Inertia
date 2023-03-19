using System;

namespace Inertia.Scriptable
{
    public abstract class ScriptableData : IDisposable
    {
        internal event BasicAction<ScriptableData>? Destroying;

        public bool IsDisposed { get; private set; }
        
        internal bool CanBeProcessed
        {
            get
            {
                var elapsed = DateTime.Now - CreatedAt;
                return elapsed >= RuntimeManager.ScriptableDataSleepTime;
            }
        }
        internal bool DisposeRequested { get; private set; }
        internal DateTime CreatedAt { get; private set; }

        public ScriptableData()
        {
            var component = RuntimeManager.GetScriptableSystem(GetType());
            if (component != null) component.RegisterComponentData(this);

            CreatedAt = DateTime.Now;
        }

        public void Dispose()
        {
            OnDispose(true);
        }
        
        internal void Destroy()
        {
            if (!DisposeRequested) return;

            Destroying?.Invoke(this);
            DisposeRequested = false;
        }

        private void OnDispose(bool disposing)
        {
            if (!IsDisposed && disposing)
            {
                IsDisposed = true;
                DisposeRequested = true;
            }
        }
    }
}