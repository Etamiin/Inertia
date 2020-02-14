using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Network
{
    public abstract class NetworkUser
    {
        #region Public variables

        public Server Server { get; private set; }
        public NetworkProtocole Protocole
        {
            get
            {
                return Server.Protocole;
            }
        }
        public IUserCore UserCore { get; set; }

        #endregion

        #region Constructors

        public NetworkUser(Server server)
        {
            Server = server;
        }

        #endregion

        public T GetUserCore<T>() where T : IUserCore
        {
            if (UserCore == null)
                return default;

            return (T)UserCore;
        }

        public abstract void Disconnect(NetworkDisconnectReason reason);
        public abstract void Send(byte[] data);

        public void Send<T>(params object[] values) where T : NetworkPacket
        {
            NetworkMessage.Create<T>(values)
                .AddUser(this)
                .SendAndDispose();
        }

        protected void BaseDispose()
        {
            Server = null;
        }
    }
}
