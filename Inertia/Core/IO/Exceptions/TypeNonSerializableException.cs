using System;

namespace Inertia
{
    /// <summary>
    /// This exception is thrown when trying to serialize a non-serializable object
    /// </summary>
    public class TypeNonSerializableException<T> : Exception
    {
        #region Public variables

        /// <summary>
        /// Message of the exception
        /// </summary>
        public override string Message => GetMessage();
        /// <summary>
        /// Object that caused the exception
        /// </summary>
        public T Value { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize a new instance of the class <see cref="TypeNonSerializableException{T}"/>
        /// </summary>
        /// <param name="value">The value that caused the exception</param>
        public TypeNonSerializableException(T value)
        {
            Value = value;
        }

        #endregion

        private string GetMessage()
        {
            return
                "The type: " + Value.GetType().Name + " isn't serializable.";
        }
    }
}
