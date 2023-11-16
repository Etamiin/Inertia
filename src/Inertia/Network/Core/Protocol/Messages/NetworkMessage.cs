namespace Inertia.Network
{
    public abstract class NetworkMessage : IAutoSerializable, ISerializableObject
    {
        public abstract ushort MessageId { get; }

        public virtual void Serialize(BasicWriter writer)
        {
            writer.SetAutoSerializable(this);
        }
        public virtual void Deserialize(BasicReader reader)
        {
            reader.GetAutoSerializable(this);
        }
    }
}