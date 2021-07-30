using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Network
{
    /// <summary>
    /// Enumerate all the disconnections reasons for the networking system
    /// </summary>
    public enum NetworkDisconnectReason : byte
    {
        /// <summary>
        /// Manual disconnection
        /// </summary>
        ManuallyDisconnect,
        /// <summary>
        /// Connection lost
        /// </summary>
        ConnectionLost,
        /// <summary>
        /// Connection failed
        /// </summary>
        ConnectionFailed
    }
}
