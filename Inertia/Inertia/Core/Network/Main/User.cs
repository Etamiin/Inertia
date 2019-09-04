using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Inertia
{
    public class UserTransmissionControl : TransmissionControl.User
    {
        internal UserTransmissionControl(ServerBase Server, Socket Socket) : base(Server, Socket)
        {
        }
    }
    public class DatagramUser : UserDatagram.User
    {
        internal DatagramUser(ServerBase Server, IPEndPoint EndPoint) : base(Server, EndPoint)
        {
        }
    }
}
