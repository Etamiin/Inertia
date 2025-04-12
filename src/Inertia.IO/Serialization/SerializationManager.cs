using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Inertia.IO
{
    internal static class SerializationManager
    {
        private static readonly ConcurrentDictionary<Type, SerializedObjectMetadata> _serializedObjectMetadatas;

        static SerializationManager()
        {
            _serializedObjectMetadatas = new ConcurrentDictionary<Type, SerializedObjectMetadata>();
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
                    metadata.Properties.Add(ReducePropertyName(property.Name, metadata.Properties), propertyMetadata);
                }
                else
                {
                    metadata.Properties.Add(property.Name, propertyMetadata);
                }                
            }

            _serializedObjectMetadatas.TryAdd(type, metadata);

            return metadata;
        }
        internal static SerializedObjectMetadata GetSerializableObjectMetadata(Type type)
        {
            return _serializedObjectMetadatas.GetOrAdd(type, RegisterSerializableObject);
        }

        private static string ReducePropertyName(string name, Dictionary<string, SerializedPropertyMetadata> properties)
        {
            var reducedName = name.Substring(0, 1);

            for (var i = 1; i < name.Length; i++)
            {
                if (!properties.ContainsKey(name)) break;

                reducedName += name[i];

                if (i == name.Length - 1)
                {
                    throw new InvalidOperationException($"Cannot reduce property name '{name}', conflict at '{reducedName}'.");
                }
            }

            return reducedName;
        }
    }
}