namespace Inertia.Network
{
    public abstract class NetworkConnectionEntity : INetworkEntity
    {
        public uint Id { get; internal set; }
        public object? State { get; set; }

        internal ServerMessageQueue AssignedMessageQueue => _messageQueue;

        private readonly ServerMessageQueue _messageQueue;
        private protected readonly NetworkEntityParameters _parameters;

        protected NetworkConnectionEntity(uint id, NetworkEntityParameters parameters)
        {
            Id = id;
            _parameters = parameters;
            _messageQueue = NetworkProtocolFactory.ServerAsyncPool.RegisterConnection(this);
        }

        public T GetStateAs<T>()
        {
            if (State is T tState) return tState;

            return default;
        }

        public void Send(NetworkMessage message)
        {
            Send(_parameters.Protocol.SerializeMessage(message));
        }

        public abstract void Send(byte[] dataToSend);
        public abstract bool Disconnect(NetworkDisconnectReason reason);
    }
}