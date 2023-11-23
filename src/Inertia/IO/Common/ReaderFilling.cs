using System;
using System.Collections.Generic;
using System.Text;

namespace Inertia.IO
{
    public sealed class ReaderFilling : IDisposable
    {
        public byte[] Data { get; set; }
        public CompressionAlgorithm CompressionAlgorithm { get; set; }
        public string? EncryptionKey { get; set; }

        private bool _isDisposed;

        public ReaderFilling(byte[] data) : this(data, CompressionAlgorithm.None, null)
        {
        }
        public ReaderFilling(byte[] data, CompressionAlgorithm binaryTransformType) : this(data, binaryTransformType, null)
        {
        }
        public ReaderFilling(byte[] data, string? encryptionKey) : this(data, CompressionAlgorithm.None, encryptionKey)
        {
        }
        public ReaderFilling(byte[] data, CompressionAlgorithm binaryTransformType, string? encryptionKey)
        {
            Data = data;
            CompressionAlgorithm = binaryTransformType;
            EncryptionKey = encryptionKey;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            if (disposing)
            {
                Data = null;
                _isDisposed = true;
            }
        }
    }
}
