using System;
using System.Threading.Tasks;

namespace Inertia.Network
{
<<<<<<< HEAD
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
=======
    public abstract class NetworkServerEntity
    {
        public bool IsDisposed { get; protected private set; }

        protected private readonly IdProvider _idProvider;
        protected private readonly string _targetIp;
        protected private readonly int _targetPort;
        protected private bool _closeNotified;
        
        protected NetworkServerEntity(string ip, int port)
        {
            _idProvider = IdProvider.CreateNew();
            _targetIp = ip.Replace("localhost", "127.0.0.1");
            _targetPort = port;
>>>>>>> premaster
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

<<<<<<< HEAD
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
=======
        protected virtual void OnStarted() { }
        protected virtual void OnClosed(NetworkDisconnectReason reason) { }
        protected virtual void BeforeDispose() { }
    }
}
>>>>>>> premaster
