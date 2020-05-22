using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Network
{
    /// <summary>
    /// Represent the base class for network server class
    /// </summary>
    public abstract class NetServer
    {
        #region Events

        /// <summary>
        /// Occurs when the server is started
        /// </summary>
        public event SimpleAction Started = () => { };
        /// <summary>
        /// Occurs when the server is closed
        /// </summary>
        public event NetworkDisconnectHandler Closed = (reason) => { };

        #endregion

        #region Public variables

        /// <summary>
        /// Return true if the current instance is disposed
        /// </summary>
        public bool IsDisposed { get; protected private set; }

        #endregion

        #region Private variables

        protected private string m_targetIp;
        protected private int m_targetPort;
        protected private bool m_closed;

        #endregion

        #region Constructors

        /// <summary>
        /// Instantiate a new instance of the class <see cref="NetServer"/>
        /// </summary>
        /// <param name="ip">Ip to use</param>
        /// <param name="port">Port to use</param>
        public NetServer(string ip, int port)
        {
            if (ip == "localhost")
                ip = "127.0.0.1";

            m_targetIp = ip;
            m_targetPort = port;
        }

        #endregion

        /// <summary>
        /// Start the server
        /// </summary>
        public abstract void Start();
        /// <summary>
        /// Close the server
        /// </summary>
        /// <param name="reason">Reason for closing the server</param>
        public abstract void Close(NetworkDisconnectReason reason = NetworkDisconnectReason.ManuallyDisconnect);

        /// <summary>
        /// Start asynchronously the server
        /// </summary>
        public void StartAsync()
        {
            Task.Factory.StartNew(() => Start());
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
