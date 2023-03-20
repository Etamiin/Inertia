using System.Threading.Tasks;

namespace Inertia.Network
{
    public abstract class NetworkServerEntity
    {
        public bool IsDisposed { get; protected private set; }

        protected private readonly SafeOrderedIntProvider _idProvider;
        protected private readonly string _ip;
        protected private readonly int _port;
        protected private bool _closeNotified;
        
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

        protected virtual void OnStarted() { }
        protected virtual void OnClosed(NetworkDisconnectReason reason) { }
    }
}