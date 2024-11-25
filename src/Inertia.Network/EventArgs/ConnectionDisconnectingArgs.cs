using Inertia.Network;
using System;

namespace Inertia
{
    internal class ConnectionDisconnectingArgs : EventArgs
    {
        internal NetworkConnectionEntity Connection { get; private set; }
        internal NetworkDisconnectReason Reason { get; private set; }

        internal ConnectionDisconnectingArgs(NetworkConnectionEntity connection, NetworkDisconnectReason reason)
        {
            Connection = connection;
            Reason = reason;
        }
    }
}
