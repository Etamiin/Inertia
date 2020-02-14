using Inertia.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Network
{
    public class UdpNetworkServer : Server, IDisposable
    {
        #region Events

        public event UdpServerAddClientHandler ClientConnected = (user) => { };
        public event UdpServerRemoveClientHandler ClientDisconnected = (user, reason) => { };

        #endregion

        #region Public variables

        public override NetworkProtocole Protocole => NetworkProtocole.UserDatagram;
        public override bool IsConnected
        {
            get
            {
                return UdpServer != null && ServerLaunched;
            }
        }
        public override int UserCount
        {
            get
            {
                if (UserDatagramUsers == null)
                    return 0;

                return UserDatagramUsers.Count;
            }
        }

        #endregion

        #region Private variables

        private UdpClient UdpServer;
        private Dictionary<IPEndPoint, UdpNetworkUser> UserDatagramUsers;
        private bool ServerLaunched;

        #endregion

        #region Constructors

        public UdpNetworkServer(string ip, int port) : base(ip, port)
        {
        }

        #endregion

        public override void Open()
        {
            if (IsConnected)
                return;

            try
            {
                UserDatagramUsers = new Dictionary<IPEndPoint, UdpNetworkUser>();
                UdpServer = new UdpClient(new IPEndPoint(IPAddress.Parse(NetworkIp), NetworkPort));
                UdpServer.BeginReceive(new AsyncCallback(OnReceiveData), UdpServer);

                ServerLaunched = true;
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
                UdpServer?.Close();
                UdpServer?.Dispose();
                

                if (UserDatagramUsers != null)
                {
                    UdpNetworkUser[] users;

                    lock (UserDatagramUsers)
                    {
                        users = UserDatagramUsers.Values.ToArray();
                        UserDatagramUsers.Clear();
                    }

                    foreach (var user in users)
                        user.Dispose();
                }
            }
            catch { }

            UserDatagramUsers = null;
            OnClose(reason);
        }
        internal override void ReceiveUserDisconnection(NetworkUser user, NetworkDisconnectReason reason)
        {
            var udpUser = (UdpNetworkUser)user;
            if (UserDatagramUsers != null)
            {
                lock (UserDatagramUsers)
                    UserDatagramUsers.Remove(udpUser.EndPoint);
            }

            ClientDisconnected(udpUser, reason);
        }

        internal void SendTo(IPEndPoint endPoint, byte[] data)
        {
            if (!IsConnected || data.Length > ushort.MaxValue)
                return;

            try { UdpServer.Send(data, data.Length, endPoint); } catch { }
        }

        private void OnReceiveData(IAsyncResult iar)
        {
            try
            {
                IPEndPoint endPoint = null;
                var data = ((UdpClient)iar.AsyncState).EndReceive(iar, ref endPoint);

                UserDatagramUsers.TryGetValue(endPoint, out UdpNetworkUser user);

                if (user == null)
                {
                    user = new UdpNetworkUser(this, endPoint);

                    UserDatagramUsers.Add(user.EndPoint, user);
                    ClientConnected(user);
                }

                InternalNetworkProcessor.OnReceiveServerData(user, data);
                UdpServer.BeginReceive(new AsyncCallback(OnReceiveData), UdpServer);
            }
            catch (Exception e)
            {
                if (!(e is SocketException || e is ObjectDisposedException) && IsConnected)
                    UdpServer.BeginReceive(new AsyncCallback(OnReceiveData), UdpServer);
            }
        }

        public void Dispose()
        {
            Close(NetworkDisconnectReason.Manual);
            BaseDispose();
            UdpServer = null;
            ClientConnected = null;
            ClientDisconnected = null;
        }
    }
}
