using System;

namespace Inertia
{
    /// <summary>
    /// This exception is thrown when trying to serialize a non-serializable object
    /// </summary>
    public class TypeNonSerializableException : Exception
    {
        /// <summary>
        ///
        /// </summary>
        public override string Message => $"The type: { ValueType.Name } isn't serializable.";
        /// <summary>
        /// <see cref="Type"/> that caused the exception
        /// </summary>
        public Type ValueType { get; private set; }

        /// <summary>
        /// Initialize a new instance of the class <see cref="TypeNonSerializableException"/>
        /// </summary>
        /// <param name="type"></param>
        public TypeNonSerializableException(Type type)
        {
            ValueType = type;
        }
    }
}
