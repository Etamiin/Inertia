namespace Inertia
{
    public interface ISerializable
    {
        void Serialize(DataWriter writer);
        void Deserialize(DataReader reader);
    }
}
