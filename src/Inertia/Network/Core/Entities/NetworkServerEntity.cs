using Inertia.Logging;
using System.Threading.Tasks;

namespace Inertia.Network
{
    public abstract class NetworkServerEntity<TParameters> where TParameters : NetworkEntityParameters
    {
        protected ILogger Logger => _parameters.Logger;
        protected NetworkProtocol Protocol => _parameters.Protocol;
        private protected readonly SafeOrderedIntProvider _idProvider;
        private protected readonly TParameters _parameters;

        protected NetworkServerEntity(TParameters parameters)
        {
            _idProvider = new SafeOrderedIntProvider();
            _parameters = parameters;
        }

        public async Task StartAsync()
        {
            await Task.Run(Start).ConfigureAwait(false);
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