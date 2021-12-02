using System;

namespace Inertia.Network
{
    public abstract class NetworkConnectionEntity : IDisposable
    {
        public bool IsDisposed { get; private set; }

        public abstract void Send(byte[] data);
        public abstract void Send(NetworkMessage message);

        public void Dispose()
        {
            Dispose(true);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                IsDisposed = true;
            }
        }
    }
}
