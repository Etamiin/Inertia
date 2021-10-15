using System;
using System.Threading.Tasks;

namespace Inertia.Network
{
    public abstract class NetworkServerEntity : IDisposable
    {
        /// <summary>
        /// Occurs when the server is started.
        /// </summary>
        protected BasicAction Started { get; set; }
        /// <summary>
        /// Occurs when the server is closed.
        /// </summary>
        protected BasicAction<NetworkDisconnectReason> Closed { get; set; }

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
                    Started = null;
                    Closed = null;
                }

                IsDisposed = true;
            }
        }
    }
}
