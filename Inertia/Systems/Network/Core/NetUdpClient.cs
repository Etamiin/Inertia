using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Network
{
    /// <summary>
    /// Represent the UDP client
    /// </summary>
    public class NetUdpClient : NetClient, IDisposable
    {
        #region Public variables

        /// <summary>
        /// Return true if the client is connected
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return (m_client?.Client) != null && m_client.Client.Connected;
            }
        }

        #endregion

        #region Private variables

        private UdpClient m_client;
        
        #endregion

        #region Constructors

        /// <summary>
        /// Instantiate a new instance of the class <see cref="NetUdpClient"/>
        /// </summary>
        /// <param name="ip">Ip to use</param>
        /// <param name="port">Port to use</param>
        public NetUdpClient(string ip, int port) : base(ip, port)
        {
        }

        #endregion

        /// <summary>
        /// Connect the client
        /// </summary>
        public override void Connect()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(NetUdpClient));

            try
            {
                m_disconnected = false;
                m_client = new UdpClient();
                m_client.Connect(new IPEndPoint(IPAddress.Parse(m_targetIp), m_targetPort));
                m_client.BeginReceive(new AsyncCallback(OnReceiveData), m_client);

                OnConnected();
            }
            catch
            {
                Disconnect(NetworkDisconnectReason.ConnectionFailed);
            }
        }
        /// <summary>
        /// Disconnect the client
        /// </summary>
        /// <param name="reason">Reason for disconnection</param>
        public override void Disconnect(NetworkDisconnectReason reason = NetworkDisconnectReason.ManuallyDisconnect)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(NetUdpClient));

            if (!IsConnected && m_disconnected)
                return;

            try
            {
                m_client.Close();
            }
            catch { }

            m_disconnected = true;
            OnDisconnected(reason);
        }
        /// <summary>
        /// Send the specified byte array to the server
        /// </summary>
        /// <param name="data">Byte array to send</param>
        public override void Send(byte[] data)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(NetUdpClient));

            if (!IsConnected)
                return;

            if (data.Length > ushort.MaxValue)
                throw new UserDatagramDataLengthLimitException(data.Length);

            m_client.SendAsync(data, data.Length);
        }

        /// <summary>
        /// Dispose the current instance
        /// </summary>
        public void Dispose()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(NetUdpClient));

            Disconnect(NetworkDisconnectReason.ManuallyDisconnect);
            m_client = null;
            IsDisposed = true;
        }

        private void OnReceiveData(IAsyncResult iar)
        {
            if (!IsConnected)
                return;

            try
            {
                IPEndPoint endPoint = null;
                var data = ((UdpClient)iar.AsyncState).EndReceive(iar, ref endPoint);

                NetworkProtocol.GetProtocol().OnReceiveData(this, new BasicReader(data));
            }
            catch (Exception e)
            {
                if (e is SocketException || e is ObjectDisposedException)
                {
                    Disconnect(NetworkDisconnectReason.ConnectionLost);
                    return;
                }
            }

            if (IsConnected)
                m_client.BeginReceive(new AsyncCallback(OnReceiveData), m_client);
        }
    }
}
