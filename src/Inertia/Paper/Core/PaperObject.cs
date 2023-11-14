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
        
        public PaperObjectState State { get; internal set; }

        public void Begin()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }

            if (State == PaperObjectState.Initialized) return;

            var pen = PaperFactory.GetPenSystem(GetType());
            if (pen != null)
            {
                State = PaperObjectState.Initialized;
                pen.ArchivePaper(this);
            }
            else Dispose();
        }
        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (IsDisposed) return;

            if (disposing)
            {
                State = PaperObjectState.Disposing;
            }
        }
    }
}