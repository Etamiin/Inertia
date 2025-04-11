using System;
using System.Threading.Tasks;

namespace Inertia.Network
{
    public abstract class ClientEntity : NetworkEntity, IClientEntity
    {
        private protected ClientProcessingQueue _processingQueue { get; }

        protected ClientEntity(string ip, int port, NetworkProtocol networkProtocol, ClientProcessingQueue processingQueue) : base(networkProtocol)
        {
            if (processingQueue == null)
            {
                throw new ArgumentNullException(nameof(processingQueue));
            }

            Ip = ip;
            Port = port;
            _processingQueue = processingQueue;
        }

        public string Ip { get; }
        public int Port { get; }
        public abstract bool IsConnected { get; }
        public object? State { get; set; }

        public abstract void Connect();
        public Task ConnectAsync()
        {
            return Task.Run(Connect);
        }

        protected virtual void OnConnected() { }
        protected virtual void OnDisconnecting(NetworkStopReason reason) { }
    }
}