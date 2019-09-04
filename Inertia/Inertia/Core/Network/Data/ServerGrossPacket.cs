using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia
{
    internal class ServerGrossPacket
    {
        public readonly byte[] Data;
        public readonly UserDatagram.User Udp;
        public readonly TransmissionControl.User Tcp;
        public readonly Reader Reader;

        public ServerGrossPacket(UserDatagram.User user, byte[] data)
        {
            Data = data;
            Udp = user;
            Reader = user.Reader;
        }
        public ServerGrossPacket(TransmissionControl.User user, byte[] data)
        {
            Data = data;
            Tcp = user;
            Reader = user.Reader;
        }
    }
}
