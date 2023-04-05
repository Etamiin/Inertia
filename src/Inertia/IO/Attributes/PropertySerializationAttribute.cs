using System;

namespace Inertia
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class PropertySerializationAttribute : Attribute
    {
        internal readonly string SerializationMethodName;
        internal readonly string DeserializationMethodName;

        public PropertySerializationAttribute(string serializationMethodName, string deserializationMethodName)
        {
            SerializationMethodName = serializationMethodName;
            DeserializationMethodName = deserializationMethodName;
        }
    }
}
