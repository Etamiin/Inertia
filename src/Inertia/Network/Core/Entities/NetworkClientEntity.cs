using Inertia.Logging;
using System;
using System.Threading.Tasks;

namespace Inertia.Network
{
    public abstract class NetworkClientEntity : NetworkEntity
    {
        public abstract bool IsConnected { get; }

        protected ILogger Logger => _parameters.Logger;

        protected NetworkProtocol Protocol => _parameters.Protocol;
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

        public async Task ConnectAsync()
        {
            await Task.Run(Connect).ConfigureAwait(false);
        }

        protected virtual void OnConnected() { }
        protected virtual void OnDisconnecting(NetworkDisconnectReason reason) { }
    }
}