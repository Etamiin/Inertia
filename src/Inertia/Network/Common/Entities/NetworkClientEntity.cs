using Inertia.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Inertia.Network
{
    public abstract class NetworkClientEntity : INetworkEntity
    {
        public abstract bool IsConnected { get; }

        protected ILogger Logger => _parameters.Logger;
        private protected NetworkProtocol Protocol => _parameters.Protocol;
        private protected readonly ClientParameters _parameters;

        protected NetworkClientEntity(ClientParameters parameters)
        {
            _parameters = parameters;
        }

        void INetworkEntity.ProcessInQueue(BasicAction action)
        {
            _parameters.ExecutionQueue.Enqueue(action);
        }

        public async Task ConnectAsync()
        {
            await Task.Run(Connect).ConfigureAwait(false);
        }
        public bool Disconnect()
        {
            return Disconnect(NetworkDisconnectReason.Manual);
        }
        public void Send(NetworkMessage message)
        {
            Send(Protocol.SerializeMessage(message));
        }
        public void SendAsync(NetworkMessage message)
        {
            ThreadPool.QueueUserWorkItem((state) => Send(message));
        }
        public void SendAsync(byte[] dataToSend)
        {
            ThreadPool.QueueUserWorkItem((state) => Send(dataToSend));
        }

        public abstract void Connect();
        public abstract bool Disconnect(NetworkDisconnectReason reason);
        public abstract void Send(byte[] data);

        protected virtual void Connected() { }
        protected virtual void Disconnecting(NetworkDisconnectReason reason) { }
    }
}