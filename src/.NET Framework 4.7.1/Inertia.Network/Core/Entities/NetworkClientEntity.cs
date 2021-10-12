using System;
using System.Threading.Tasks;

namespace Inertia.Network
{
    /// <summary>
    ///
    /// </summary>
    public abstract class NetworkClientEntity : IDisposable
    {
        /// <summary>
        /// Occurs when the client is connected.
        /// </summary>
        public event BasicAction Connected = () => { };
        /// <summary>
        /// Occurs when the client is disconnected.
        /// </summary>
        public event NetworkDisconnectHandler Disconnected = (reason) => { };

        /// <summary>
        /// Returns true if the current instance is disposed
        /// </summary>
        public bool IsDisposed { get; protected private set; }

        protected private readonly string _targetIp;
        protected private readonly int _targetPort;
        protected private bool _disconnectNotified;

        /// <summary>
        /// Instantiate a new instance of the class <see cref="NetworkClientEntity"/>.
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public NetworkClientEntity(string ip, int port)
        {
            _targetIp = ip.Replace("localhost", "127.0.0.1");
            _targetPort = port;

            DefaultNetworkProtocol.Initialize();
        }

        /// <summary>
        /// Returns true if the connection is active otherwise false.
        /// </summary>
        /// <returns></returns>
        public abstract bool IsConnected();
        /// <summary>
        /// Start the connection with the indicated ip and port.
        /// </summary>
        public abstract void Connect();
        /// <summary>
        /// Terminate the connection with the indicated reason.
        /// </summary>
        /// <param name="reason"></param>
        public abstract void Disconnect(NetworkDisconnectReason reason = NetworkDisconnectReason.Manual);

        /// <summary>
        /// Sends the indicated data through the current connection.
        /// </summary>
        /// <param name="data"></param>
        public abstract void Send(byte[] data);
        /// <summary>
        /// Start the connection asynchronously with the indicated ip and port.
        /// </summary>
        public void ConnectAsync()
        {
            Task.Factory.StartNew(() => Connect());
        }
        /// <summary>
        /// Sends the specified NetworkMessage through the current connection.
        /// </summary>
        /// <param name="message"></param>
        public void Send(NetworkMessage message)
        {
            Send(NetworkProtocol.GetProtocol().OnParseMessage(message));
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
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

        internal void OnConnected()
        {
            Connected();
        }
        internal void OnDisconnected(NetworkDisconnectReason reason)
        {
            Disconnected(reason);
        }
    }
}
