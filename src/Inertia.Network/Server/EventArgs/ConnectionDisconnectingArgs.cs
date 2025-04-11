using Inertia.Network;
using System;

namespace Inertia
{
    internal class ConnectionDisconnectingArgs : EventArgs
    {
        internal ConnectionDisconnectingArgs(ConnectionEntity connection, NetworkStopReason reason)
        {
            Connection = connection;
            Reason = reason;
        }

        internal ConnectionEntity Connection { get; }
        internal NetworkStopReason Reason { get; }
    }
}
