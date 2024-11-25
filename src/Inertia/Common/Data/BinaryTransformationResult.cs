using System;

namespace Inertia.IO
{
    public sealed class BinaryTransformationResult : IDisposable
    {
        public bool IsDisposed { get; private set; }
        public bool IsSuccess { get; private set; }
        
        private byte[]? _data;
        private Exception? _error;

        internal BinaryTransformationResult(bool isSuccess, byte[]? data, Exception? error)
        {
            IsSuccess = isSuccess;
            _data = data;
            _error = error;
        }

        public byte[] GetDataOrThrow()
        {
            this.ThrowIfDisposable(IsDisposed);

            if (_error != null) throw _error;
            if (_data is null) throw new ArgumentNullException(nameof(_data));

            return _data;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (!IsDisposed && disposing)
            {
                _data = null;
                _error = null;

                IsDisposed = true;
            }
        }
    }
}