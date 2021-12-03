using System;
using System.Threading.Tasks;

namespace Inertia.Network
{
    public abstract class NetworkClientEntity
    {
        public bool IsDisposed { get; protected private set; }

        public abstract bool IsConnected { get; }

        protected private readonly string _targetIp;
        protected private readonly int _targetPort;
        protected private bool _disconnectNotified;

        internal NetworkClientEntity(string ip, int port)
        {
            _targetIp = ip.Replace("localhost", "127.0.0.1");
            _targetPort = port;
        }

        public abstract void Connect();
        public abstract void Disconnect(NetworkDisconnectReason reason);
        public abstract void Send(byte[] data);

        public void ConnectAsync()
        {
            Task.Factory.StartNew(Connect);
        }
        public void Disconnect()
        {
            Disconnect(NetworkDisconnectReason.Manual);
        }
        public void Send(NetworkMessage message)
        {
            Send(NetworkProtocol.GetCurrentProtocol().OnSerializeMessage(message));
        }

        protected virtual void OnConnected() { }
        protected virtual void OnDisconnected(NetworkDisconnectReason reason) { }
        protected virtual void BeforeDispose() { }
    }
}