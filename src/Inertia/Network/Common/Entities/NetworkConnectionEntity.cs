namespace Inertia.Network
{
    public abstract class NetworkConnectionEntity : INetworkEntity
    {
        public uint Id { get; internal set; }

        internal ServerMessageQueue AssignedMessageQueue => _messageQueue;

        private readonly ServerMessageQueue _messageQueue;
        private protected NetworkProtocol _protocol { get; private set; }

        protected NetworkConnectionEntity(uint id, NetworkProtocol protocol)
        {
            Id = id;
            _protocol = protocol;
            _messageQueue = NetworkProtocolFactory.ServerAsyncPool.RegisterConnection(this);
        }

        public void Send(NetworkMessage message)
        {
            Send(_protocol.SerializeMessage(message));
        }

        public abstract void Send(byte[] data);
        public abstract bool Disconnect(NetworkDisconnectReason reason);
    }
}