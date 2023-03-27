using Inertia.Logging;
using System.Threading.Tasks;

namespace Inertia.Network
{
    public abstract class NetworkServerEntity
    {
        protected ILogger? Logger => Parameters.Logger;

        protected readonly SafeOrderedIntProvider IdProvider;
        protected readonly ServerParameters Parameters;

        protected NetworkServerEntity(ServerParameters parameters)
        {
            IdProvider = new SafeOrderedIntProvider();
            Parameters = parameters;
        }

        public void StartAsync()
        {
            Task.Run(Start);
        }        
        public void Close()
        {
            Close(NetworkDisconnectReason.Manual);
        }

        public abstract void Start();
        public abstract void Close(NetworkDisconnectReason reason);

        protected virtual void Started() { }
        protected virtual void Closed(NetworkDisconnectReason reason) { }
    }
}