﻿using System;

namespace Inertia.IO
{
    public sealed class ZipCompressionResult : IDisposable
    {
        public bool IsDisposed { get; private set; }
        public bool? HasBetterSize { get; private set; }
        public byte[] Data { get; private set; }

        internal ZipCompressionResult(byte[] data, bool? hasBetterSize)
        {
            Data = data;
            HasBetterSize = hasBetterSize;
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
            }

            IsDisposed = true;
        }
    }
}