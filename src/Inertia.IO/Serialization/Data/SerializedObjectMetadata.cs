using System;
using System.Collections.Generic;
using System.Reflection;

namespace Inertia.IO
{
    internal class SerializedObjectMetadata
    {
        internal SerializedObjectMetadata(Type type)
        {
            Properties = new Dictionary<string, SerializedPropertyMetadata>();
            Options = type.GetCustomAttribute<ObjectSerializationOptionsAttribute>();

            if (Options is null)
            {
                Options = new ObjectSerializationOptionsAttribute(false);
            }
        }

        internal Dictionary<string, SerializedPropertyMetadata> Properties { get; }
        internal ObjectSerializationOptionsAttribute Options { get; }
    }
}
