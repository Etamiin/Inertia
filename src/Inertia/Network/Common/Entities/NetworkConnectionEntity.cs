using Inertia.Logging;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Inertia.Network
{
    public abstract class NetworkConnectionEntity : NetworkEntity, INetworkConnectionWrapper
    {
        public uint Id { get; internal set; }
        public object? State { get; set; }

        protected ILogger Logger => _parameters.Logger;

        private protected ServerMessageQueue _messageQueue { get; private set; }
        private protected readonly NetworkEntityParameters _parameters;

        protected NetworkConnectionEntity(uint id, NetworkEntityParameters parameters)
        {
            Id = id;
            _parameters = parameters;
            _messageQueue = NetworkProtocolFactory.ServerAsyncPool.RegisterConnection(this);
        }

        internal override void ProcessInQueue(BasicAction action)
        {
            _messageQueue?.Enqueue(action);
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
        public void SendAsync(NetworkMessage message)
        {
            ThreadPool.QueueUserWorkItem((_) => Send(message));
        }
        public void SendAsync(byte[] dataToSend)
        {
            ThreadPool.QueueUserWorkItem((_) => Send(dataToSend));
        }

        public abstract void Send(byte[] dataToSend);
        public abstract bool Disconnect(NetworkDisconnectReason reason);
    }
}