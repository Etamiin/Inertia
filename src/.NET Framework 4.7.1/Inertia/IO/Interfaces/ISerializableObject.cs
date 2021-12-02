namespace Inertia
{
<<<<<<< HEAD
<<<<<<< HEAD
    /// <summary>
    /// 
    /// </summary>
    public interface ISerializableObject
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        void Serialize(BasicWriter writer);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
=======
    public interface ISerializableObject
    {
        void Serialize(BasicWriter writer);
>>>>>>> 9bfc85f6784b254a10c65f104446a83c8b195c40
=======
    public interface ISerializableObject
    {
        void Serialize(BasicWriter writer);
>>>>>>> premaster
        void Deserialize(BasicReader reader);
    }
}
