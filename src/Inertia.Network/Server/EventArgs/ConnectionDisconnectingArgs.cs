using Inertia.Network;
using System;

namespace Inertia
{
    internal class ConnectionDisconnectingArgs : EventArgs
    {
        internal ConnectionDisconnectingArgs(TcpConnectionEntity connection, NetworkStopReason reason)
        {
            Connection = connection;
            Reason = reason;
        }

        internal TcpConnectionEntity Connection { get; }
        internal NetworkStopReason Reason { get; }
    }
}
