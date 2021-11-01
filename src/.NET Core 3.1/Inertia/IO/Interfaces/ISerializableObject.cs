namespace Inertia
{
    public interface ISerializableObject
    {
        void Serialize(BasicWriter writer);
        void Deserialize(BasicReader reader);
    }
}
