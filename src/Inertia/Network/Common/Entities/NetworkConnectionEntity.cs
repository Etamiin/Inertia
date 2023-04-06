using Inertia.Logging;

namespace Inertia.Network
{
    public abstract class NetworkConnectionEntity : INetworkEntity
    {
        public uint Id { get; internal set; }
        public object? State { get; set; }

        protected ILogger Logger => _parameters.Logger;

        internal ServerMessageQueue AssignedMessageQueue { get; private set; }

        private protected readonly NetworkEntityParameters _parameters;

        protected NetworkConnectionEntity(uint id, NetworkEntityParameters parameters)
        {
            Id = id;
            _parameters = parameters;
            AssignedMessageQueue = NetworkProtocolFactory.ServerAsyncPool.RegisterConnection(this);
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