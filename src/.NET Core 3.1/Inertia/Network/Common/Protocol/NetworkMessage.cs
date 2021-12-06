using System;

namespace Inertia.Network
{
    public abstract class NetworkMessage : IAutoSerializable, ISerializableObject
    {
        public abstract ushort MessageId { get; }

        public virtual bool UseAutoSerialization { get; } = true;

        public virtual void Serialize(BasicWriter writer)
        {
            if (UseAutoSerialization)
            {
                writer.SetAutoSerializable(this);
            }
        }
        public virtual void Deserialize(BasicReader reader)
        {
            if (UseAutoSerialization)
            {
                reader.GetAutoSerializable(this);
            }
        }
    }
}
