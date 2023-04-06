using System;

namespace Inertia.Paper
{
    public abstract class PaperObject : IDisposable
    {
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
                component.RegisterPaper(this);
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