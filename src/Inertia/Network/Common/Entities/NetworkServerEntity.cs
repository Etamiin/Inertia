using Inertia.Logging;
using System.Threading.Tasks;

namespace Inertia.Network
{
    public abstract class NetworkServerEntity<TParameters> where TParameters : ServerParameters
    {
        public readonly TParameters Parameters;

        protected ILogger? Logger => Parameters.Logger;

        private protected readonly SafeOrderedIntProvider IdProvider;
        
        protected NetworkServerEntity(TParameters parameters)
        {
            IdProvider = new SafeOrderedIntProvider();
            Parameters = parameters;
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