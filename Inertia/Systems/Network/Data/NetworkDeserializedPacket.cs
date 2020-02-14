using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inertia.Network;

namespace Inertia.Internal
{
    internal class NetworkDeserializedPacket : IDisposable
    {
        #region Public variables

        public NetworkPacket Packet { get; private set; }
        public InertiaReader ContentReader { get; private set; }

        #endregion

        #region Constructors

        internal NetworkDeserializedPacket(NetworkPacket packet, byte[] content)
        {
            Packet = packet;
            ContentReader = new InertiaReader(content);
        }

        #endregion

        public void Dispose()
        {
            if (ContentReader == null)
                return;

            ContentReader.Dispose();
            ContentReader = null;
            Packet = null;
        }
    }
}
