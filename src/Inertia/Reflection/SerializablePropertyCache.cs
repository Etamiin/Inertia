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

        public bool WriteTo(object serializableObject, DataWriter writer)
        {
            var value = _property.GetValue(serializableObject);

            if (value != null)
            {
                writer
                    .Write(_property.Name)
                    .Write(value, _property.PropertyType);

                return true;
            }

            return false;
        }
        public void ReadFrom(object serializableObject, DataReader reader)
        {
            _property.SetValue(serializableObject, reader.ReadValue(_property.PropertyType));
        }
    }
}
