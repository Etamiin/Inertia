using System;

namespace Inertia
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class PropertySerialization : Attribute
    {
        internal readonly string SerializationMethodName;
        internal readonly string DeserializationMethodName;

        public PropertySerialization(string serializationMethodName, string deserializationMethodName)
        {
            SerializationMethodName = serializationMethodName;
            DeserializationMethodName = deserializationMethodName;
        }
    }
}
