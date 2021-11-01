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
        public event BasicAction Started;
        /// <summary>
        /// Occurs when the server is closed.
        /// </summary>
        public event NetworkDisconnectHandler Closed;

        /// <summary>
        /// Returns true if the current instance is disposed.
        /// </summary>
        public bool IsDisposed { get; protected private set; }

        protected private string _targetIp;
        protected private int _targetPort;
        protected private bool _closeNotified;

        /// <summary>
        /// Instantiate a new instance of the class <see cref="NetworkServerEntity"/>
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        protected NetworkServerEntity(string ip, int port)
        {
            _targetIp = ip.Replace("localhost", "127.0.0.1");
            _targetPort = port;

            DefaultNetworkProtocol.Initialize();
        }

        /// <summary>
        /// Start asynchronously the server.
        /// </summary>
        public void StartAsync()
        {
            Task.Factory.StartNew(Start);
        }
        /// <summary>
        /// Close the server.
        /// </summary>
        public void Close()
        {
            Close(NetworkDisconnectReason.Manual);
        }

        /// <summary>
        /// Start the server.
        /// </summary>
        public abstract void Start();
        /// <summary>
        /// Close the server with the specified reason.
        /// </summary>
        /// <param name="reason"></param>
        public abstract void Close(NetworkDisconnectReason reason);

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
                    Started = null;
                    Closed = null;
                }

                IsDisposed = true;
            }
        }

        internal void OnStarted()
        {
            Started?.Invoke();
        }
        internal void OnClosed(NetworkDisconnectReason reason)
        {
            Closed?.Invoke(reason);
        }
    }
}
