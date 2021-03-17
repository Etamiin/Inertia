using System;

namespace Inertia
{
    /// <summary>
    /// This exception is thrown when trying to serialize a too big bool flag
    /// </summary>
    public class BoolFlagTooLargeException : Exception
    {
        /// <summary>
        ///
        /// </summary>
        public override string Message => "Can't serialize more than 8 booleans in a bool flag";

        /// <summary>
        /// Initialize a new instance of the class <see cref="BoolFlagTooLargeException"/>
        /// </summary>
        /// <param name="type"></param>
        public BoolFlagTooLargeException()
        {
        }
    }
}
