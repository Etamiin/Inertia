using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Network
{
    public abstract class NetworkProcessor
    {
        #region Constructors

        public NetworkProcessor(object inProcessor)
        {
        }

        #endregion

        public abstract void ProcessClientData(Client client, ref NetworkProcessData data);
        public abstract void ProcessServerData(NetworkUser user, ref NetworkProcessData data);
        public abstract byte[] BuildClientMessage(NetworkMessage message);
        public abstract byte[] BuildServerMessage(NetworkMessage message);
    }
}
