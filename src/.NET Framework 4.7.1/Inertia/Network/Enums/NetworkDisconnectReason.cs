﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Network
{
    public enum NetworkDisconnectReason : byte
    {
        Manual,
        ConnectionLost,
        ConnectionFailed
    }
}
