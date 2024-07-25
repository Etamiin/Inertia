using System;
using System.Threading.Tasks;

namespace Inertia.Network
{
    public abstract class NetworkEntity
    {
        private protected readonly NetworkEntityParameters _parameters;

        protected NetworkEntity(NetworkEntityParameters parameters)
        {
            _parameters = parameters;
        }

        public abstract void Send(byte[] dataToSend);
        public abstract bool Disconnect(NetworkDisconnectReason reason);

        public virtual void Send(NetworkMessage message)
        {
            Send(_parameters.Protocol.SerializeMessage(message));
        }
        public bool Disconnect()
        {
            return Disconnect(NetworkDisconnectReason.Manual);
        }

        public Task SendAsync(NetworkMessage message)
        {
            return Task.Run(() => Send(message));
        }
        public Task SendAsync(byte[] dataToSend)
        {
            return Task.Run(() => Send(dataToSend));
        }

        internal abstract void ProcessInQueue(Action action);
    }
}