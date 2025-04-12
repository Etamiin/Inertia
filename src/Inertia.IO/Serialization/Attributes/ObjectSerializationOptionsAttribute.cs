using System;

namespace Inertia.IO
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ObjectSerializationOptionsAttribute : Attribute
    {
        public ObjectSerializationOptionsAttribute(bool reducePropertyName)
        {
            ReducePropertyName = reducePropertyName;
        }

        public bool ReducePropertyName { get; }
    }
}
