using Inertia.IO;

namespace Inertia.Network
{
    [ObjectSerializationOptions(true)]
    public abstract class NetworkMessage
    {
        public abstract ushort MessageId { get; }
    }
}