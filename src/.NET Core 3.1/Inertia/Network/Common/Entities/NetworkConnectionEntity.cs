using System;

namespace Inertia.Network
{
    public abstract class NetworkConnectionEntity
    {
        public bool IsDisposed { get; protected private set; }
        public uint Id { get; internal set; }

        internal ServerMessageQueue AssignedMessageQueue { get; set; }

        protected internal NetworkConnectionEntity(uint id)
        {
            Id = id;
            NetworkProtocol.ServerAsyncPool.RegisterConnection(this);
        }

        public abstract void Send(byte[] data);
        public abstract void Send(NetworkMessage message);
    }
}