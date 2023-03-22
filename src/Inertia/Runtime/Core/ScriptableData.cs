using System;

namespace Inertia.Scriptable
{
    public abstract class ScriptableData : IDisposable
    {
        internal enum ScriptableDataState : byte
        {
            NoState = 0,
            Initializing = 1,
            Initialized = 2,
            Disposing = 3,
            Disposed = 4
        }

        internal event BasicAction<ScriptableData>? Destroying;

        public bool IsDisposed { get; private set; }
        
        internal ScriptableDataState State { get; set; }

        public ScriptableData()
        {
            var component = RuntimeManager.GetScriptableSystem(GetType());
            if (component != null)
            {
                component.RegisterComponentData(this);

                State = ScriptableDataState.Initializing;
            }
        }

        public void Dispose()
        {
            OnDispose(true);
        }
        
        internal void Destroy()
        {
            if (State != ScriptableDataState.Disposing) return;

            Destroying?.Invoke(this);
            State = ScriptableDataState.Disposed;
        }

        private void OnDispose(bool disposing)
        {
            if (!IsDisposed && disposing)
            {
                IsDisposed = true;
                State = ScriptableDataState.Disposing;
            }
        }
    }
}