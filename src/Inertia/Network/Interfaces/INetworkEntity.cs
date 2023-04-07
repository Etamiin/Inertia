using System;

namespace Inertia.Network
{
    public interface INetworkEntity
    {
        internal void ProcessInQueue(BasicAction action);
    }
}
