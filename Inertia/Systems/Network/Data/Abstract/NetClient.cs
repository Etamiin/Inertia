using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Network
{
    /// <summary>
    /// Represent the base class for network client class
    /// </summary>
    public abstract class NetClient
    {
        #region Events

        /// <summary>
        /// Occurs when the client is connected
        /// </summary>
        public event SimpleAction Connected = () => { };
        /// <summary>
        /// Occurs when the client is disconnected
        /// </summary>
        public event NetworkDisconnectHandler Disconnected = (reason) => { };

        #endregion

        #region Public variables

        /// <summary>
        /// Return true if the current instance is disposed
        /// </summary>
        public bool IsDisposed { get; protected private set; }

        #endregion

        #region Private variables

        protected private readonly string m_targetIp;
        protected private readonly int m_targetPort;
        protected private bool m_disconnected;

        #endregion

        #region Constructors

        /// <summary>
        /// Instantiate a new instance of the class <see cref="NetClient"/>
        /// </summary>
        /// <param name="ip">Ip to use</param>
        /// <param name="port">Port to use</param>
        public NetClient(string ip, int port)
        {
            if (ip == "localhost")
                ip = "127.0.0.1";

            m_targetIp = ip;
            m_targetPort = port;
        }

        #endregion

        /// <summary>
        /// Connect the client
        /// </summary>
        public abstract void Connect();
        /// <summary>
        /// Disconnect the client
        /// </summary>
        /// <param name="reason"></param>
        public abstract void Disconnect(NetworkDisconnectReason reason = NetworkDisconnectReason.ManuallyDisconnect);

        /// <summary>
        /// Send the specified byte array to the server
        /// </summary>
        /// <param name="data"></param>
        public abstract void Send(byte[] data);

        /// <summary>
        /// Connect the client asynchronously
        /// </summary>
        public void ConnectAsync()
        {
            Task.Factory.StartNew(() => Connect());
        }
        /// <summary>
        /// Send the specified <see cref="NetPacket"/> to the server
        /// </summary>
        /// <param name="packet">Packet to send</param>
        public void Send(NetPacket packet)
        {
            if (packet is CustomNetPacket)
                Send(NetworkProtocol.Protocol.OnParsePacket((CustomNetPacket)packet));
            else
                Send(NetworkProtocol.Protocol.OnParsePacket(packet));
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
