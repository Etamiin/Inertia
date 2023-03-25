using System.Threading.Tasks;

namespace Inertia.Network
{
    public abstract class NetworkServerEntity
    {
        protected readonly SafeOrderedIntProvider IdProvider;
        protected readonly string Ip;
        protected readonly int Port;

        protected NetworkServerEntity(string ip, int port)
        {
            IdProvider = new SafeOrderedIntProvider();
            Ip = ip.Replace("localhost", "127.0.0.1");
            Port = port;
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