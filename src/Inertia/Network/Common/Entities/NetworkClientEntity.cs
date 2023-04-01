using Inertia.Logging;
using System.Threading.Tasks;

namespace Inertia.Network
{
    public abstract class NetworkClientEntity : INetworkEntity
    {
        public abstract bool IsConnected { get; }

        protected ILogger? Logger => _parameters.Logger;
        protected NetworkProtocol Protocol => _parameters.Protocol;
        private protected readonly ClientParameters _parameters;
        
        protected NetworkClientEntity(ClientParameters parameters)
        {
            _parameters = parameters;
        }

        public abstract void Connect();
        public abstract bool Disconnect(NetworkDisconnectReason reason);
        public abstract void Send(byte[] data);

        public void ConnectAsync()
        {
            Task.Run(Connect);
        }
        public bool Disconnect()
        {
            return Disconnect(NetworkDisconnectReason.Manual);
        }
        public void Send(NetworkMessage message)
        {
            Send(Protocol.SerializeMessage(message));
        }

        protected virtual void Connected() { }
        protected virtual void Disconnecting(NetworkDisconnectReason reason) { }
    }
}