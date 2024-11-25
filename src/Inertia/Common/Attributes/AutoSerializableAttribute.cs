using System;

namespace Inertia
{
    /// <summary>
    /// Marks a class as automatically serializable.
    /// </summary>
    /// <remarks>
    /// This attribute can be applied to classes to indicate that they should be automatically
    /// serialized by the Inertia framework.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class AutoSerializableAttribute : Attribute
    {
    }
}
