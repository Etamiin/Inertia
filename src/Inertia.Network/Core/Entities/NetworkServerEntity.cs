using Inertia.Logging;
using System.Threading.Tasks;

namespace Inertia.Network
{
    public abstract class NetworkServerEntity<TParameters> where TParameters : NetworkEntityParameters
    {
        protected ILogger Logger => _parameters.Logger;
        private protected readonly SafeOrderedIntProvider _idProvider;
        private protected readonly TParameters _parameters;

        protected NetworkServerEntity(TParameters parameters)
        {
            _idProvider = new SafeOrderedIntProvider();
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