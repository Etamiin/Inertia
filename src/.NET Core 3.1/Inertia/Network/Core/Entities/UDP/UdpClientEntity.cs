using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Inertia.Network
{
<<<<<<< HEAD
    public class UdpClientEntity : NetworkClientEntity, IDisposable
=======
    public abstract class UdpClientEntity : NetworkClientEntity, IDisposable
>>>>>>> premaster
    {
        public override bool IsConnected => (_client?.Client) != null && _client.Client.Connected;
        private UdpClient _client;

<<<<<<< HEAD
        /// <summary>
        /// Instantiate a new instance of the class <see cref="UdpClientEntity"/>
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
=======
>>>>>>> premaster
        public UdpClientEntity(string ip, int port) : base(ip, port)
        {
        }
        
<<<<<<< HEAD
        public UdpClientEntity CatchOnConnected(BasicAction callback)
        {
            Connected = callback;
            return this;
        }
        public UdpClientEntity CatchOnDisconnected(BasicAction<NetworkDisconnectReason> callback)
        {
            Disconnected = callback;
            return this;
        }

        public override void Connect()
=======
        public sealed override void Connect()
>>>>>>> premaster
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(UdpClientEntity));
            }

            if (!IsConnected)
            {
                try
                {
                    _disconnectNotified = false;
                    _client = new UdpClient();
                    _client.Connect(new IPEndPoint(IPAddress.Parse(_targetIp), _targetPort));

<<<<<<< HEAD
                    Connected?.Invoke();
=======
                    OnConnected();
>>>>>>> premaster
                    _client.BeginReceive(new AsyncCallback(OnReceiveData), _client);
                }
                catch
                {
                    Disconnect(NetworkDisconnectReason.ConnectionFailed);
                }
            }
        }        
<<<<<<< HEAD
        public override void Disconnect(NetworkDisconnectReason reason)
=======
        public sealed override void Disconnect(NetworkDisconnectReason reason)
>>>>>>> premaster
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(UdpClientEntity));
            }

            if (IsConnected)
            {
                _client?.Close();
            }
            if (!_disconnectNotified)
            {
                _disconnectNotified = true;
<<<<<<< HEAD
                Disconnected.Invoke(reason);
            }
        }
        
        public override void Send(byte[] data)
=======
                OnDisconnected(reason);
            }
        }
        public sealed override void Send(byte[] data)
>>>>>>> premaster
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(UdpClientEntity));
            }

<<<<<<< HEAD
            if (data.Length > ushort.MaxValue)
            {
                throw new UserDatagramDataLengthLimitException();
            }

            try { _client.SendAsync(data, data.Length); } catch { }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Disconnect(NetworkDisconnectReason.Manual);
                _client?.Dispose();
            }

            base.Dispose(disposing);
=======
            if (IsConnected)
            {
                if (data.Length > ushort.MaxValue)
                {
                    throw new UserDatagramDataLengthLimitException();
                }

                _client.SendAsync(data, data.Length);
            }
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                BeforeDispose();
                Disconnect(NetworkDisconnectReason.Manual);
                _client?.Dispose();

                IsDisposed = true;
            }
>>>>>>> premaster
        }

        private void OnReceiveData(IAsyncResult iar)
        {
            if (IsConnected)
            {
                try
                {
                    IPEndPoint endPoint = null;
                    var data = ((UdpClient)iar.AsyncState).EndReceive(iar, ref endPoint);

<<<<<<< HEAD
                    NetworkProtocol.GetProtocol().OnReceiveData(this, new BasicReader(data));
=======
                    NetworkProtocol.ProcessParsing(this, new BasicReader(data));
>>>>>>> premaster
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
                {
                    _client.BeginReceive(new AsyncCallback(OnReceiveData), _client);
                }
            }
        }
    }
}
