using System;
using System.Threading.Tasks;

namespace Inertia.Network
{
    public abstract class NetworkServerEntity
    {
        public bool IsDisposed { get; protected private set; }

        protected private readonly SafeOrderedIntProvider _idProvider;
        protected private readonly string _targetIp;
        protected private readonly int _targetPort;
        protected private bool _closeNotified;
        
        protected internal NetworkServerEntity(string ip, int port)
        {
            _idProvider = new SafeOrderedIntProvider();
            _targetIp = ip.Replace("localhost", "127.0.0.1");
            _targetPort = port;
        }

        public void StartAsync()
        {
            Task.Factory.StartNew(Start);
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