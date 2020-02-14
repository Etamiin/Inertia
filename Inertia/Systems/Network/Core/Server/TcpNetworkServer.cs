using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Network
{
    public class TcpNetworkServer : Server, IDisposable
    {
        #region Events

        public event TcpServerAddClientHandler ClientConnected = (user) => { };
        public event TcpServerRemoveClientHandler ClientDisconnected = (user, reason) => { };

        #endregion

        #region Public variables

        public override NetworkProtocole Protocole => NetworkProtocole.TransmissionControl;
        public override bool IsConnected
        {
            get
            {
                return Server != null && Server.IsBound;
            }
        }
        public override int UserCount
        {
            get
            {
                if (TransmissionControlUsers.Count == 0)
                    return 0;

                return TransmissionControlUsers.Count;
            }
        }

        #endregion

        #region Private variables

        private Socket Server;
        private int UserInstanceId;
        private Dictionary<int, TcpNetworkUser> TransmissionControlUsers;

        #endregion

        #region Constructors

        public TcpNetworkServer(string ip, int port) : base(ip, port)
        {
        }

        #endregion

        public override void Open()
        {
            if (IsConnected)
                return;

            try
            {
                TransmissionControlUsers = new Dictionary<int, TcpNetworkUser>();
                Server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Server.Bind(new IPEndPoint(IPAddress.Parse(NetworkIp), NetworkPort));
                Server.Listen(1000);
                Server.BeginAccept(new AsyncCallback(OnAcceptConnection), Server);

                OnOpen();
            }
            catch
            {
                Close(NetworkDisconnectReason.Failed);
            }
        }
        public override void Close(NetworkDisconnectReason reason)
        {
            if (!IsConnected)
                return;

            try
            {
                Server?.Close();
                Server?.Dispose();
                Server = null;

                if (TransmissionControlUsers != null)
                {
                    TcpNetworkUser[] users = null;

                    lock (TransmissionControlUsers)
                    {
                        users = TransmissionControlUsers.Values.ToArray();
                        TransmissionControlUsers.Clear();
                    }

                    foreach (var user in users)
                        user.Dispose();
                }
            }
            catch { }

            TransmissionControlUsers = null;
            OnClose(reason);
        }
        internal override void ReceiveUserDisconnection(NetworkUser user, NetworkDisconnectReason reason)
        {
            var tcpUser = (TcpNetworkUser)user;
            if (TransmissionControlUsers != null)
            {
                lock (TransmissionControlUsers)
                    TransmissionControlUsers.Remove(tcpUser.Id);
            }

            ClientDisconnected(tcpUser, reason);
        }

        private void OnAcceptConnection(IAsyncResult iar)
        {
            try
            {
                var socket = ((Socket)iar.AsyncState).EndAccept(iar);
                var user = new TcpNetworkUser(this, socket, ++UserInstanceId);

                TransmissionControlUsers.Add(user.Id, user);
                ClientConnected(user);
                Server.BeginAccept(new AsyncCallback(OnAcceptConnection), Server);
            }
            catch (Exception e)
            {
                if (!(e is SocketException || e is ObjectDisposedException) && IsConnected)
                    Server.BeginAccept(new AsyncCallback(OnAcceptConnection), Server);
            }
        }

        public void Dispose()
        {
            Close(NetworkDisconnectReason.Manual);
            BaseDispose();
            ClientConnected = null;
            ClientDisconnected = null;
        }
    }
}
