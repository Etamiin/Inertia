using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Network
{
    public enum NetworkDisconnectReason : byte
    {
        Manual = 1,
        ConnectionLost = 2,
        ConnectionFailed = 4,
        SendingBadInformation = 8,
        Spamming = 16
    }
}
