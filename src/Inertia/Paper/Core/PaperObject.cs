using System;

namespace Inertia.Scriptable
{
    public abstract class PaperObject : IDisposable
    {
        internal enum PaperObjectState : byte
        {
            NoState = 0,
            Initialized = 1,
            Disposing = 2,
            Disposed = 3
        }

        public bool IsDisposed
        {
            get
            {
                return State == PaperObjectState.Disposing || State == PaperObjectState.Disposed;
            }
        }
        
        internal PaperObjectState State { get; set; }

        public void SetActive()
        {
            if (IsDisposed || State == PaperObjectState.Initialized) return;

            var component = PaperFactory.GetScriptableSystem(GetType());
            if (component != null)
            {
                State = PaperObjectState.Initialized;
                component.RegisterComponentData(this);
            }
            else Dispose();
        }
        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        { 
            if (disposing)
            {
                State = PaperObjectState.Disposing;
            }
        }
    }
}