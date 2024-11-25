namespace Inertia
{
    public interface ISerializable
    {
        byte Version { get; }

        void Serialize(DataWriter writer);
        void Deserialize(byte version, DataReader reader);
    }
}