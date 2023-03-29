using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Inertia.Network
{
    [Obsolete]
    public abstract class UdpServerEntity : NetworkServerEntity<ServerParameters>, IDisposable
    {
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

        private readonly Dictionary<IPEndPoint, UdpConnectionEntity> _connections;
        private UdpClient _client;
        private BasicReader _reader;

        protected UdpServerEntity(ServerParameters parameters) : base(parameters)
        {
            _connections = new Dictionary<IPEndPoint, UdpConnectionEntity>();
        }

        public sealed override void Start()
        {
            //if (IsDisposed)
            //{
            //    throw new ObjectDisposedException(nameof(UdpServerEntity));
            //}

            if (!IsInitialized)
            {
                try
                {
                    _reader = new BasicReader();
                    _connections.Clear();
                    _client = new UdpClient(new IPEndPoint(string.IsNullOrWhiteSpace(Parameters.Ip) ? IPAddress.Any : IPAddress.Parse(Parameters.Ip), Parameters.Port));

                    OnStarted();
                    _client.BeginReceive(new AsyncCallback(OnReceiveData), _client);
                }
                catch
                {
                    Close(NetworkDisconnectReason.ConnectionFailed);
                }
            }
        }        
        public sealed override void Close(NetworkDisconnectReason reason)
        {
            //if (IsDisposed)
            //{
            //    throw new ObjectDisposedException(nameof(UdpServerEntity));
            //}

            if (IsInitialized)
            {
                _reader?.Dispose();
                _client?.Close();
                _client = null;
            }
            //if (!_closeNotified)
            //{
            //    _connections.Clear();
            //    _closeNotified = true;

            //    Closed(reason);
            //}
        }

        public void SendTo(UdpConnectionEntity connection, byte[] data)
        {
            //if (connection.IsDisposed)
            //{
            //    throw new ObjectDisposedException(nameof(UdpConnectionEntity));
            //}

            SendTo(connection.EndPoint, data);
        }        
        public void SendTo(UdpConnectionEntity connection, NetworkMessage message)
        {
            //if (connection.IsDisposed)
            //{
            //    throw new ObjectDisposedException(nameof(UdpConnectionEntity));
            //}

            SendTo(connection.EndPoint, NetworkProtocol.Current.SerializeMessage(message));
        }

        public virtual void OnConnectionAdded(UdpConnectionEntity connection) { }

        public void Dispose()
        {
            //if (!IsDisposed)
            //{
            //    Close();
            //    _client?.Dispose();

            //    IsDisposed = true;
            //}
        }

        private void SendTo(IPEndPoint endPoint, byte[] data)
        {
            //if (IsDisposed)
            //{
            //    throw new ObjectDisposedException(nameof(UdpServerEntity));
            //}

            if (data.Length > ushort.MaxValue)
            {
                throw new UserDatagramDataLengthLimitException();
            }

            try
            {
                _client.Send(data, data.Length, endPoint);
            }
            catch { }
        }
        private void OnReceiveData(IAsyncResult iar)
        {
            try
            {
                IPEndPoint endPoint = null;
                var data = ((UdpClient)iar.AsyncState).EndReceive(iar, ref endPoint);

                if (!_connections.ContainsKey(endPoint))
                {
                    var connection = new UdpConnectionEntity((uint)IdProvider.NextValue(), this, endPoint);
                    _connections.Add(endPoint, connection);

                    OnConnectionAdded(connection);
                }

                NetworkProtocol.ProcessParsing(_connections[endPoint], _reader.Fill(data));
            }
            catch (Exception ex)
            {
                if (ex is SocketException || ex is ObjectDisposedException)
                {
                    return;
                }
            }

            if (IsInitialized)
            {
                _client.BeginReceive(new AsyncCallback(OnReceiveData), _client);
            }
        }
    }
}