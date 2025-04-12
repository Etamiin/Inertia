using System.Threading.Tasks;

namespace Inertia.Network
{
    public abstract class ServerEntity
    {
        protected ServerEntity(string ip, int port)
        {
            Ip = ip;
            Port = port;
        }

        protected string Ip { get; }
        protected int Port { get; }

        public abstract void Start();
        public abstract void Stop(NetworkStopReason reason);
        
        public Task StartAsync()
        {
            return Task.Run(Start);
        }
        public void Stop()
        {
            Stop(NetworkStopReason.Manual);
        }

        protected virtual void OnStarted() { }
        protected virtual void OnStopped(NetworkStopReason reason) { }
    }
}