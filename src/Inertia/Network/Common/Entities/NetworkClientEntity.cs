using System.Threading.Tasks;

namespace Inertia.Network
{
    public abstract class NetworkClientEntity
    {
        public abstract bool IsConnected { get; }

        protected readonly string Ip;
        protected readonly int Port;

        protected NetworkClientEntity(string ip, int port)
        {
            Ip = ip.Replace("localhost", "127.0.0.1");
            Port = port;
        }

        public abstract void Connect();
        public abstract bool Disconnect(NetworkDisconnectReason reason);
        public abstract void Send(byte[] data);

        public void ConnectAsync()
        {
            Task.Run(Connect);
        }
        public bool Disconnect()
        {
            return Disconnect(NetworkDisconnectReason.Manual);
        }
        public void Send(NetworkMessage message)
        {
            Send(NetworkProtocol.Current.SerializeMessage(message));
        }

        protected virtual void Connected() { }
        protected virtual void Disconnecting(NetworkDisconnectReason reason) { }
    }
}