using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Network
{
    public abstract class NetworkPacket
    {
        #region Public variables

        public abstract uint Id { get; }

        #endregion

        public virtual void OnClientReceive(Client client, InertiaReader reader)
        {
        }
        public virtual void OnServerReceive(NetworkUser user, InertiaReader reader)
        {
        }
    }
}
