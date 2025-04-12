using System.Text;

namespace Inertia.IO
{
    public sealed class PooledDataWriter : DataWriter
    {
        internal PooledDataWriter() { }
        internal PooledDataWriter(int capacity) : base(capacity) { }
        internal PooledDataWriter(Encoding encoding) : base(encoding) { }
        internal PooledDataWriter(Encoding encoding, int capacity) : base(encoding, capacity) { }

        internal bool IsPooled { get; set; }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed && disposing)
            {
                IsPooled = false;

                DataWriterPool.Return(this);
            }
        }
    }
}
