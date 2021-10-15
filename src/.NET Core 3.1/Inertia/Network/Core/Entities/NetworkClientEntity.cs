using System;
using System.Threading.Tasks;

namespace Inertia.Network
{
    public abstract class NetworkClientEntity : IDisposable
    {
        /// <summary>
        /// Occurs when the client is connected.
        /// </summary>
        protected BasicAction Connected { get; set; }
        /// <summary>
        /// Occurs when the client is disconnected.
        /// </summary>
        protected BasicAction<NetworkDisconnectReason> Disconnected { get; set; }

        public bool IsDisposed { get; protected private set; }

        public abstract bool IsConnected { get; }

        protected private readonly string _targetIp;
        protected private readonly int _targetPort;
        protected private bool _disconnectNotified;

        /// <summary>
        /// Instantiate a new instance of the class <see cref="NetworkClientEntity"/>.
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        protected NetworkClientEntity(string ip, int port)
        {
            _targetIp = ip.Replace("localhost", "127.0.0.1");
            _targetPort = port;

            DefaultNetworkProtocol.Initialize();
        }

        public void Disconnect()
        {
            Disconnect(NetworkDisconnectReason.Manual);
        }

        public abstract void Connect();        
        public abstract void Disconnect(NetworkDisconnectReason reason);

        public abstract void Send(byte[] data);
        public void Send(NetworkMessage message)
        {
            Send(NetworkProtocol.GetProtocol().OnParseMessage(message));
        }

        public void ConnectAsync()
        {
            Task.Factory.StartNew(Connect);
        }
        
        public void Dispose()
        {
            Dispose(true);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    Connected = null;
                    Disconnected = null;
                }

                IsDisposed = true;
            }
        }
    }
}
