namespace Inertia.Network
{
    public abstract class NetworkConnectionEntity
    {
        public uint Id { get; internal set; }

        internal readonly ServerMessageQueue AssignedMessageQueue;

        private protected NetworkConnectionEntity(uint id)
        {
            Id = id;
            AssignedMessageQueue = NetworkProtocol.ServerAsyncPool.RegisterConnection(this);
        }

        public abstract void Send(byte[] data);
        public abstract void Send(NetworkMessage message);
    }
}