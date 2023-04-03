namespace Inertia.Network
{
    public abstract class NetworkConnectionEntity : INetworkEntity
    {
        public uint Id { get; internal set; }

        internal ServerMessageQueue AssignedMessageQueue => _messageQueue;

        private readonly ServerMessageQueue _messageQueue;
        private protected readonly NetworkEntityParameters _parameters;

        protected NetworkConnectionEntity(uint id, NetworkEntityParameters parameters)
        {
            Id = id;
            _parameters = parameters;
            _messageQueue = NetworkProtocolFactory.ServerAsyncPool.RegisterConnection(this);
        }

        public void Send(NetworkMessage message)
        {
            Send(_parameters.Protocol.SerializeMessage(message));
        }

        public abstract void Send(byte[] data);
        public abstract bool Disconnect(NetworkDisconnectReason reason);
    }
}