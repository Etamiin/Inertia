using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inertia.Internal;

namespace Inertia.Network
{
    public class NetworkMessage : IDisposable
    {
        #region Static methods

        public static NetworkMessage Create<T>(params object[] values) where T : NetworkPacket
        {
            return new NetworkMessage(typeof(T), values);
        }

        #endregion

        #region Public variables

        public NetworkPacket Packet { get; private set; }
        public int Length
        {
            get
            {
                return (int)Writer.TotalLength;
            }
        }

        #endregion

        #region Internal variables

        internal InertiaWriter Writer;

        #endregion

        #region Events

        private event NetworkMessageSenderHandler ClientSending = (data) => { };
        private event NetworkMessageSenderHandler UserSending = (data) => { };
        private event NetworkMessageSenderHandler GroupSending = (data) => { };

        #endregion

        #region Private variables

        private bool IsForClient, IsForUser;

        #endregion

        #region Constructors

        internal NetworkMessage(Type packetType, object[] values)
        {
            Packet = InternalNetworkProcessor.GetPacket(packetType);
            Writer = new InertiaWriter();
            
            AddValues(values);
        }

        #endregion
        
        public byte[] Export()
        {
            return Writer.Export();
        }

        public NetworkMessage AddValue(object value)
        {
            Writer.SetValue(value);
            return this;
        }
        public NetworkMessage AddValues(params object[] values)
        {
            Writer.SetValues(values);
            return this;
        }

        public NetworkMessage AddClient(Client client)
        {
            ClientSending += client.Send;
            IsForClient = true;
            return this;
        }
        public NetworkMessage AddClients(params Client[] clients)
        {
            foreach (var client in clients)
                AddClient(client);
            return this;
        }

        public NetworkMessage AddUser(NetworkUser user)
        {
            UserSending += user.Send;
            IsForUser = true;
            return this;
        }
        public NetworkMessage AddUsers(params NetworkUser[] users)
        {
            foreach (var user in users)
                AddUser(user);
            return this;
        }

        public NetworkMessage AddUserGroup(NetworkUserGroup group)
        {
            GroupSending += group.Send;
            IsForUser = true;
            return this;
        }

        public void Send()
        {
            if (Packet == null)
                throw new ObjectDisposedException(nameof(NetworkMessage));

            if (IsForClient)
                ClientSending(InertiaConfiguration.NetworkProcessor.BuildClientMessage(this));

            if (IsForUser)
            {
                var data = InertiaConfiguration.NetworkProcessor.BuildServerMessage(this);

                UserSending(data);
                GroupSending(data);
            }
        }
        public void SendAndDispose()
        {
            Send();
            Dispose();
        }

        public void Dispose()
        {
            if (Writer != null)
            {
                Writer.Dispose();
                Writer = null;
            }

            Packet = null;
            ClientSending = null;
            UserSending = null;
            GroupSending = null;
        }
    }
}
