using Inertia.Logging;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Inertia.Network
{
    public abstract class NetworkClientEntity : NetworkEntity
    {
        public abstract bool IsConnected { get; }

        protected ILogger Logger => _parameters.Logger;

        protected NetworkProtocol Protocol => _parameters.Protocol;
        private protected readonly ClientParameters _parameters;

        protected NetworkClientEntity(ClientParameters parameters)
        {
            _parameters = parameters;
        }

        internal override void ProcessInQueue(Action action)
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
            ThreadPool.QueueUserWorkItem((_) => Send(message));
        }
        public void SendAsync(byte[] dataToSend)
        {
            ThreadPool.QueueUserWorkItem((_) => Send(dataToSend));
        }

        public abstract void Connect();
        public abstract bool Disconnect(NetworkDisconnectReason reason);
        public abstract void Send(byte[] data);

        protected virtual void Connected() { }
        protected virtual void Disconnecting(NetworkDisconnectReason reason) { }
    }
}