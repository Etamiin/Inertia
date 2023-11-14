using System;

namespace Inertia.IO
{
    public sealed class AesEncryptionResult : IDisposable
    {
        public bool IsDisposed { get; private set; }
        public bool Success { get; private set; }
        public byte[]? Data { get; private set; }
        public Exception? Error { get; private set; }

        internal AesEncryptionResult(bool success, byte[]? data, Exception? error)
        {
            Success = success;
            Data = data;
            Error = error;
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