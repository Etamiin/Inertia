using System;

namespace Inertia.IO
{
    public sealed class BinaryTransformationResult : IDisposable
    {
        public static BinaryTransformationResult Success(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            return new BinaryTransformationResult(true, data, null);
        }
        public static BinaryTransformationResult Failure(Exception error)
        {
            if (error == null)
            {
                throw new ArgumentNullException(nameof(error));
            }

            return new BinaryTransformationResult(false, null, error);
        }
        
        private byte[]? _data;
        private Exception? _error;

        private BinaryTransformationResult(bool isSuccess, byte[]? data, Exception? error)
        {
            IsSuccess = isSuccess;
            _data = data;
            _error = error;
        }

        public bool IsDisposed { get; private set; }
        public bool IsSuccess { get; }

        public byte[] GetDataOrThrow()
        {
            this.ThrowIfDisposable(IsDisposed);

            if (!IsSuccess) throw _error;
            
            return _data;
        }
        public void Dispose()
        {
            if (!IsDisposed)
            {
                _data = null;
                _error = null;

                IsDisposed = true;
            }
        }
    }
}