using System.Threading.Tasks;

namespace Inertia.Network
{
    public abstract class NetworkServerEntity
    {
        protected private readonly SafeOrderedIntProvider _idProvider;
        protected private readonly string _ip;
        protected private readonly int _port;

        private protected NetworkServerEntity(string ip, int port)
        {
            _idProvider = new SafeOrderedIntProvider();
            _ip = ip.Replace("localhost", "127.0.0.1");
            _port = port;
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