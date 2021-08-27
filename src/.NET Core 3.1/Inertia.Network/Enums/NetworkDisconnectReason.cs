using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Network
{
    /// <summary>
    ///
    /// </summary>
    public enum NetworkDisconnectReason : byte
    {
        /// <summary>
        ///
        /// </summary>
        Manual,
        /// <summary>
        ///
        /// </summary>
        ConnectionLost,
        /// <summary>
        ///
        /// </summary>
        ConnectionFailed
    }
}
