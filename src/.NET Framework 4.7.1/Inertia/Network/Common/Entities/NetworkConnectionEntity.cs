using System;

namespace Inertia.Network
{
    public abstract class NetworkConnectionEntity
    {
        public bool IsDisposed { get; protected private set; }
        public uint Id { get; internal set; }

        protected internal NetworkConnectionEntity(uint id)
        {
            Id = id;
        }

        public abstract void Send(byte[] data);
        public abstract void Send(NetworkMessage message);
    }
}
