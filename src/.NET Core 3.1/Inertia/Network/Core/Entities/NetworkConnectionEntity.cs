using System;

namespace Inertia.Network
{
<<<<<<< HEAD
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
=======
    public abstract class NetworkConnectionEntity
    {
        public bool IsDisposed { get; protected private set; }
        public uint Id { get; internal set; }

        public NetworkConnectionEntity(uint id)
        {
            Id = id;
        }

        public abstract void Send(byte[] data);
        public abstract void Send(NetworkMessage message);
>>>>>>> premaster
    }
}
