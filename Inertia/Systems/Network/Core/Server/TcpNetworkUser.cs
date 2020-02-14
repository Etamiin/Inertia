using Inertia.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Network
{
    public class TcpNetworkUser : NetworkUser, IDisposable
    {
        #region Public variables

        public bool IsConnected
        {
            get
            {
                return User != null && User.Connected;
            }
        }

        #endregion

        #region Internal variables

        internal readonly int Id;
        internal InertiaReader TcpReader { get; private set; }

        #endregion

        #region Private variables

        private Socket User;
        private byte[] Buffer;

        #endregion

        #region Constructors

        internal TcpNetworkUser(Server server, Socket socket, int userId) : base(server)
        {
            User = socket;
            Id = userId;
            Buffer = new byte[InertiaConfiguration.NetworkBufferLength];
            TcpReader = new InertiaReader();
            User.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, new AsyncCallback(OnReceiveData), User);
        }

        #endregion

        public override void Disconnect(NetworkDisconnectReason reason)
        {
            if (!IsConnected)
                return;

            Server.ReceiveUserDisconnection(this, reason);
            ToTrash();
        }
        public override void Send(byte[] data)
        {
            if (!IsConnected)
                return;

            User.Send(data);
        }

        private void OnReceiveData(IAsyncResult iar)
        {
            try
            {
                var received = ((Socket)iar.AsyncState).EndReceive(iar);
                if (received == 0)
                    throw new SocketException();

                var data = new byte[received];
                Array.Copy(Buffer, data, received);

                InternalNetworkProcessor.OnReceiveServerData(this, data);
                User.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, new AsyncCallback(OnReceiveData), User);
            }
            catch (Exception e)
            {
                if (e is SocketException || e is ObjectDisposedException)
                {
                    Disconnect(NetworkDisconnectReason.Lost);
                    return;
                }
                else if (IsConnected)
                    User.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, new AsyncCallback(OnReceiveData), User);
            }
        }
        private void ToTrash()
        {
            User?.Close();
            User?.Dispose();
            User = null;
            TcpReader?.Dispose();
            TcpReader = null;
            User = null;
            Buffer = null;
        }

        public void Dispose()
        {
            if (IsConnected)
                Disconnect(NetworkDisconnectReason.Manual);
            else
                ToTrash();

            BaseDispose();
        }
    }
}
