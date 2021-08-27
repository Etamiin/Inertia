using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Inertia.Network
{
    /// <summary>
    ///
    /// </summary>
    public class UdpClientEntity : NetworkClientEntity, IDisposable
    {
        private UdpClient m_client;

        /// <summary>
        /// Instantiate a new instance of the class <see cref="UdpClientEntity"/>
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public UdpClientEntity(string ip, int port) : base(ip, port)
        {
        }

        /// <summary>
        /// Returns true if the connection is active otherwise false.
        /// </summary>
        /// <returns></returns>
        public override bool IsConnected()
        {
            return (m_client?.Client) != null && m_client.Client.Connected;
        }
        /// <summary>
        /// Start the connection with the indicated ip and port.
        /// </summary>
        public override void Connect()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(UdpClientEntity));
            if (IsConnected())
                return;

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
        /// Terminate the connection with the indicated reason.
        /// </summary>
        /// <param name="reason"></param>
        public override void Disconnect(NetworkDisconnectReason reason = NetworkDisconnectReason.Manual)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(UdpClientEntity));
            if (!IsConnected() && m_disconnected)
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
        /// Sends the indicated data through the current connection.
        /// </summary>
        /// <param name="data"></param>
        public override void Send(byte[] data)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(UdpClientEntity));
            if (m_disconnected)
                return;

            if (data.Length > ushort.MaxValue)
                throw new UserDatagramDataLengthLimitException(data.Length);

            try { m_client.SendAsync(data, data.Length); } catch { }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Disconnect(NetworkDisconnectReason.Manual);
                m_client?.Dispose();
            }

            base.Dispose(disposing);
        }

        private void OnReceiveData(IAsyncResult iar)
        {
            if (!IsConnected())
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

            if (IsConnected())
                m_client.BeginReceive(new AsyncCallback(OnReceiveData), m_client);
        }
    }
}
