namespace Inertia.Network
{
    public abstract class NetworkMessage : ISerializable
    {
        public abstract ushort MessageId { get; }

        public virtual void Serialize(DataWriter writer)
        {
            writer.WriteAutoSerializable(this);
        }
        public virtual void Deserialize(DataReader reader)
        {
            reader.ReadAutoSerializable(this);
        }
    }
}