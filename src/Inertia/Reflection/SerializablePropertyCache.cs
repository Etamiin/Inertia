using System.Reflection;

namespace Inertia
{
    internal class SerializablePropertyCache
    {
        private readonly PropertyInfo _property;

        internal SerializablePropertyCache(PropertyInfo info)
        {
            _property = info;
        }

        public void WriteTo(object serializableObject, DataWriter writer)
        {
            writer
                .Write(_property.Name)
                .Write(_property.GetValue(serializableObject), _property.PropertyType);
        }
        public void ReadFrom(object serializableObject, DataReader reader)
        {
            _property.SetValue(serializableObject, reader.ReadValue(_property.PropertyType));
        }
    }
}
