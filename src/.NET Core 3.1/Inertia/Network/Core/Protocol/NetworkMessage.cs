using System;

namespace Inertia.Network
{
    public abstract class NetworkMessage : IAutoSerializable
    {
        public abstract uint MessageId { get; }
    }
}
