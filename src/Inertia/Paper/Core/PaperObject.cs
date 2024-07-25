using System;

namespace Inertia.Paper
{
    public abstract class PaperObject : IDisposable
    {
        public bool IsDisposed
        {
            get
            {
                return State == PaperObjectState.Disposed;
            }
        }
        public PaperObjectState State { get; internal set; }
        public IPenSystem PenSystem { get; private set; }

        protected PaperObject()
        {
            PenSystem = PaperFactory.GetPenSystem(GetType());
            if (PenSystem != null)
            {
                PenSystem.RegisterPaper(this);
                State = PaperObjectState.Initialized;
            }
            else Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed) return;

            if (disposing)
            {
                State = PaperObjectState.Disposed;
                PenSystem = null;
            }
        }
    }
}