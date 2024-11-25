using Inertia.Logging;
using System;
using System.Threading.Tasks;

namespace Inertia.Network
{
    public abstract class NetworkClientEntity : NetworkEntity, INetworkClientEntity
    {
        public abstract bool IsConnected { get; }
        public object? State { get; set; }

        protected ILogger Logger => _parameters.Logger;

        private protected readonly ClientParameters _clientParameters;

        protected NetworkClientEntity(ClientParameters parameters) : base(parameters)
        {
            _clientParameters = parameters;
        }

        internal sealed override void ProcessInQueue(Action action)
        {
            _clientParameters.ExecutionQueue.Enqueue(action);
        }

        public abstract void Connect();
        public Task ConnectAsync()
        {
            return Task.Run(Connect);
        }

        protected virtual void OnConnected() { }
        protected virtual void OnDisconnecting(NetworkDisconnectReason reason) { }
    }
}