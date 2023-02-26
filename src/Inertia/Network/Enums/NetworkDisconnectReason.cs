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
        ConnectionTimeout = 8,
        SendingBadInformation = 16,
        Spamming = 32
    }
}
