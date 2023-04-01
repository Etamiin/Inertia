using Inertia.Logging;
using System.Threading.Tasks;

namespace Inertia.Network
{
    public abstract class NetworkServerEntity<TParameters> : INetworkEntity where TParameters : NetworkEntityParameters
    {
        public TParameters Parameters => _parameters;

        protected ILogger? Logger => _parameters.Logger;
        protected NetworkProtocol Protocol => _parameters.Protocol;
        private protected readonly SafeOrderedIntProvider IdProvider;
        private readonly TParameters _parameters;

        protected NetworkServerEntity(TParameters parameters)
        {
            IdProvider = new SafeOrderedIntProvider();
            _parameters = parameters;
        }

        public Task StartAsync()
        {
            return Task.Run(Start);
        }        
        public void Close()
        {
            Close(NetworkDisconnectReason.Manual);
        }

        public abstract void Start();
        public abstract void Close(NetworkDisconnectReason reason);

        protected virtual void OnStarted() { }
        protected virtual void OnClosed(NetworkDisconnectReason reason) { }
    }
}