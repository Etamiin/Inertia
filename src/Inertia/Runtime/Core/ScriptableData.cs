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
            Destroying = 3,
            Disposed = 4
        }

        internal event BasicAction<ScriptableData>? Disposing;

        public bool IsDisposed { get; private set; }
        
        internal ScriptableDataState State { get; set; }

        public ScriptableData()
        {
            State = ScriptableDataState.Initializing;

            var component = RuntimeManager.GetScriptableSystem(GetType());
            if (component != null)
            {
                component.RegisterComponentData(this);
            }
            else
            {
                BeginDestroy();
                Dispose();
            }
        }

        public void BeginDestroy()
        {
            State = ScriptableDataState.Destroying;
        }
        public void Dispose()
        {
            if (State != ScriptableDataState.Destroying) return;

            OnDispose(true);
        }       

        private void OnDispose(bool disposing)
        {
            if (!IsDisposed && disposing)
            {
                Disposing?.Invoke(this);

                IsDisposed = true;
                State = ScriptableDataState.Disposed;
            }
        }
    }
}