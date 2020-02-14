using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Network
{
    public class NetworkUserGroup : IDisposable
    {
        #region Events

        public event NetworkMessageSenderHandler GroupSending = (data) => { };

        #endregion

        #region Public variables

        public bool IsDisposed { get; private set; }
        public int Count { get; private set; }

        #endregion

        #region Constructors

        public NetworkUserGroup()
        {
        }

        #endregion

        public NetworkUserGroup Join(NetworkUser user)
        {
            GroupSending += user.Send;
            Count++;
            return this;
        }
        public NetworkUserGroup Leave(NetworkUser user)
        {
            GroupSending -= user.Send;
            Count--;
            return this;
        }
        public NetworkUserGroup Join(params NetworkUser[] users)
        {
            foreach (var user in users)
                Join(user);
            return this;
        }
        public NetworkUserGroup Leave(params NetworkUser[] users)
        {
            foreach (var user in users)
                Leave(user);
            return this;
        }

        public void Send(byte[] data)
        {
            GroupSending(data);
        }
        public void Send<T>(params object[] values) where T : NetworkPacket
        {
            var message = NetworkMessage.Create<T>(values)
                .AddUserGroup(this);

            message.Send();
            message.Dispose();
        }

        public void Dispose()
        {
            if (IsDisposed)
                return;

            IsDisposed = true;
            GroupSending = null;
        }
    }
}
