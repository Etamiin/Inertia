using System;
using System.Threading.Tasks;

namespace Inertia.Network
{
    public abstract class NetworkEntity
    {
        protected NetworkEntity(NetworkProtocol networkProtocol)
        {
            Check.ThrowsIfNull(networkProtocol, nameof(networkProtocol));

            NetworkProtocol = networkProtocol;
        }
        
        internal NetworkProtocol NetworkProtocol { get; }

        public abstract void Send(byte[] data);
        public abstract void Disconnect(NetworkStopReason reason);
        
        public void Send(NetworkMessage message)
        {
            Send(NetworkProtocol.SerializeMessage(message));
        }
        public void Disconnect()
        {
            Disconnect(NetworkStopReason.Manual);
        }

        public Task SendAsync(NetworkMessage message)
        {
            return Task.Run(() => Send(message));
        }
        public Task SendAsync(byte[] data)
        {
            return Task.Run(() => Send(data));
        }
    }
}