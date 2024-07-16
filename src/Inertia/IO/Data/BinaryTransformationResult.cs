using System;

namespace Inertia.IO
{
    public sealed class BinaryTransformationResult : IDisposable
    {
        public bool IsDisposed { get; private set; }
        public bool Success { get; private set; }
        public byte[]? Data { get; private set; }
        public Exception? Error { get; private set; }

        internal BinaryTransformationResult(bool success, byte[]? data, Exception? error)
        {
            Success = success;
            Data = data;
            Error = error;
        }

        public byte[] GetDataOrThrow()
        {
            if (Error != null) throw Error;
            if (Data == null) throw new ArgumentNullException(nameof(Data));

            return Data;
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
                Data = null;
                Error = null;
            }

            IsDisposed = true;
        }
    }
}