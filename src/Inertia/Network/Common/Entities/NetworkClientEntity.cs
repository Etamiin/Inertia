using Inertia.Logging;
using System.Threading.Tasks;

namespace Inertia.Network
{
    public abstract class NetworkClientEntity : INetworkEntity
    {
        internal protected readonly ClientParameters Parameters;

        public abstract bool IsConnected { get; }

        protected ILogger Logger => Parameters.Logger;
        protected NetworkProtocol Protocol => Parameters.Protocol;
        
        protected NetworkClientEntity(ClientParameters parameters)
        {
            Parameters = parameters;
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