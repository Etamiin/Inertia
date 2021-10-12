using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Inertia.Network
{
    /// <summary>
    ///
    /// </summary>
    public class UdpServerEntity : NetworkServerEntity, IDisposable
    {
        /// <summary>
        /// Occurs when receiving for the first time data from an udp connection.
        /// </summary>
        public event NetworkUdpConnectionAddedHandler ConnectionAdded = (connection) => { };

        /// <summary>
        /// Returns true if <see cref="Start"/> was called successfully.
        /// </summary>
        public bool IsInitialized
        {
            get
            {
                return _client != null;
            }
        }

        private UdpClient _client;
        private Dictionary<IPEndPoint, UdpConnectionEntity> _connections;

        /// <summary>
        /// Instantiate a new instance of the class <see cref="UdpServerEntity"/>
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public UdpServerEntity(string ip, int port) : base(ip, port)
        {
        }

        /// <summary>
        /// Start the server.
        /// </summary>
        public override void Start()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(UdpServerEntity));
            }

            if (!IsInitialized)
            {
                try
                {
                    _closeNotified = false;
                    _connections = new Dictionary<IPEndPoint, UdpConnectionEntity>();
                    _client = new UdpClient(new IPEndPoint(IPAddress.Parse(_targetIp), _targetPort));
                    _client.BeginReceive(new AsyncCallback(OnReceiveData), _client);

                    OnStarted();
                }
                catch
                {
                    Close(NetworkDisconnectReason.ConnectionFailed);
                }
            }
        }
        /// <summary>
        /// Close the server with the specified reason.
        /// </summary>
        /// <param name="reason"></param>
        public override void Close(NetworkDisconnectReason reason)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(UdpServerEntity));
            }

            if (IsInitialized || !_closeNotified)
            {
                try
                {
                    _client.Close();
                }
                catch { }

                _connections.Clear();
                _connections = null;
                _closeNotified = true;

                OnClosed(reason);
            }
        }

        /// <summary>
        /// Sends the specified data to the specified connection.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="data"></param>
        public void SendTo(UdpConnectionEntity connection, byte[] data)
        {
            if (connection.IsDisposed)
            {
                throw new ObjectDisposedException(nameof(UdpConnectionEntity));
            }

            SendTo(connection.EndPoint, data);
        }
        /// <summary>
        /// Sends the specified <see cref="NetworkMessage"/> to the specified connection.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="message"></param>
        public void SendTo(UdpConnectionEntity connection, NetworkMessage message)
        {
            if (connection.IsDisposed)
            {
                throw new ObjectDisposedException(nameof(UdpConnectionEntity));
            }

            SendTo(connection.EndPoint, NetworkProtocol.GetProtocol().OnParseMessage(message));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Close();
                ConnectionAdded = null;
                _client?.Dispose();
            }

            base.Dispose(disposing);
        }

        private void SendTo(IPEndPoint endPoint, byte[] data)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(UdpServerEntity));
            }

            if (IsInitialized)
            {
                if (data.Length > ushort.MaxValue)
                {
                    throw new UserDatagramDataLengthLimitException(data.Length);
                }

                try { _client.Send(data, data.Length, endPoint); } catch { }
            }
        }
        private void OnReceiveData(IAsyncResult iar)
        {
            try
            {
                IPEndPoint endPoint = null;
                var data = ((UdpClient)iar.AsyncState).EndReceive(iar, ref endPoint);

                if (!_connections.ContainsKey(endPoint))
                {
                    var connection = new UdpConnectionEntity(this, endPoint);
                    _connections.Add(endPoint, connection);

                    ConnectionAdded(connection);
                }

                NetworkProtocol.GetProtocol().OnReceiveData(_connections[endPoint], new BasicReader(data));
            }
            catch (Exception ex)
            {
                if (ex is SocketException || ex is ObjectDisposedException)
                    return;
            }

            if (IsInitialized)
            {
                _client.BeginReceive(new AsyncCallback(OnReceiveData), _client);
            }
        }
    }
}