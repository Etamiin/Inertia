using Inertia.Logging;
using System;

namespace Inertia.Network
{
    public abstract class NetworkConnectionEntity : NetworkEntity, INetworkConnectionWrapper
    {
        public uint Id { get; internal set; }
        public object? State { get; set; }

        protected ILogger Logger => _parameters.Logger;

        private protected ServerMessageQueue _messageQueue { get; private set; }

        protected NetworkConnectionEntity(uint id, NetworkEntityParameters parameters) : base(parameters)
        {
            Id = id;
            _messageQueue = NetworkProtocolManager.ServerMessagePool.RegisterConnection(this);
        }

        internal override void ProcessInQueue(Action action)
        {
            _messageQueue?.Enqueue(action);
        }
    }
}