using System;
using System.Threading.Tasks;

namespace Inertia.Network
{
    /// <summary>
    ///
    /// </summary>
    public abstract class NetworkServerEntity : IDisposable
    {
        /// <summary>
        /// Occurs when the server is started.
        /// </summary>
        public event BasicAction Started = () => { };
        /// <summary>
        /// Occurs when the server is closed.
        /// </summary>
        public event NetworkDisconnectHandler Closed = (reason) => { };

        /// <summary>
        /// Returns true if the current instance is disposed.
        /// </summary>
        public bool IsDisposed { get; protected private set; }

        protected private string m_targetIp;
        protected private int m_targetPort;
        protected private bool m_closed;

        /// <summary>
        /// Instantiate a new instance of the class <see cref="NetworkServerEntity"/>
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public NetworkServerEntity(string ip, int port)
        {
            m_targetIp = ip.Replace("localhost", "127.0.0.1");
            m_targetPort = port;

            DefaultNetworkProtocol.Initialize();
        }

        /// <summary>
        /// Start the server.
        /// </summary>
        public abstract void Start();
        /// <summary>
        /// Close the server with the specified reason.
        /// </summary>
        /// <param name="reason"></param>
        public abstract void Close(NetworkDisconnectReason reason = NetworkDisconnectReason.Manual);

        /// <summary>
        /// Start asynchronously the server.
        /// </summary>
        public void StartAsync()
        {
            Task.Factory.StartNew(() => Start());
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
            if (IsDisposed)
                return;

            if (disposing)
            {
                Started = null;
                Closed = null;
            }

            IsDisposed = true;
        }

        internal void OnStarted()
        {
            Started();
        }
        internal void OnClosed(NetworkDisconnectReason reason)
        {
            Closed(reason);
        }
    }
}
