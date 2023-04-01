using System;

namespace Inertia.IO
{
    public sealed class ZipCompressionResult : IDisposable
    {
        public bool IsDisposed { get; private set; }
        public bool? HasBetterSize { get; private set; }
        public byte[] Data
        {
            get
            {
                this.ThrowIfDisposable(IsDisposed);
                return _data;
            }
        }

        private byte[] _data;

        internal ZipCompressionResult(byte[] decompressedData)
        {
            _data = decompressedData;
            HasBetterSize = null;
        }
        internal ZipCompressionResult(byte[] compressedData, bool hasBetterSize)
        {
            _data = compressedData;
            HasBetterSize = hasBetterSize;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (!IsDisposed) return;

            if (disposing)
            {
                _data = null;
            }

            IsDisposed = true;
        }
    }
}
