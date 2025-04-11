using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Inertia.IO
{
    internal static class SerializationManager
    {
        private static readonly Dictionary<Type, SerializedObjectMetadata> _serializedObjectMetadatas;

        static SerializationManager()
        {
            _serializedObjectMetadatas = new Dictionary<Type, SerializedObjectMetadata>();
        }

        internal static SerializedObjectMetadata RegisterSerializableObject(Type type)
        {
            if (!type.IsClass || type.IsAbstract)
            {
                throw new ArgumentException("Type must be a non-abstract class.", nameof(type));
            }

            var metadata = new SerializedObjectMetadata(type);
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where((p) => p.CanRead && p.CanWrite && p.GetCustomAttribute<IgnoreAttribute>() is null)
                .OrderBy((p) => p.Name);

            foreach (var property in properties)
            {
                var propertyMetadata = new SerializedPropertyMetadata(property);

                if (metadata.Options.ReducePropertyName)
                {
                    string name = property.Name.Substring(0, 1);

                    for (var i = 1; i < property.Name.Length; i++)
                    {
                        if (metadata.Properties.ContainsKey(name))
                        {
                            name += property.Name[i];
                        }
                        else break;

                        if (i == property.Name.Length - 1)
                        {
                            throw new InvalidOperationException("Property name cannot be reduced: name is already registered.");
                        }
                    }

                    metadata.Properties.Add(name, propertyMetadata);
                }
                else
                {
                    metadata.Properties.Add(property.Name, propertyMetadata);
                }                
            }

            _serializedObjectMetadatas.Add(type, metadata);

            return metadata;
        }
        internal static SerializedObjectMetadata GetSerializableObjectMetadata(Type type)
        {
            if (!_serializedObjectMetadatas.TryGetValue(type, out var metadata))
            {
                metadata = RegisterSerializableObject(type);
            }

            return metadata;
        }
    }
}
