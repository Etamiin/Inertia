namespace Inertia.Network
{
    [AutoSerializable]
    public abstract class NetworkMessage : ISerializable
    {
        public virtual byte Version { get; } = 0;
        public abstract ushort MessageId { get; }

        public virtual void Serialize(DataWriter writer)
        {
            writer.WriteAutoSerializable(this);
        }
        public virtual void Deserialize(byte version, DataReader reader)
        {
            reader.TryReadAutoSerializable(this);
        }
    }
}