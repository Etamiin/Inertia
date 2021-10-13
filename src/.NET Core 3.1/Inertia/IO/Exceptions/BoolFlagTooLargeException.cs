using System;

namespace Inertia
{
    /// <summary>
    /// Exception thrown when flag size limit is reached
    /// </summary>
    public class BoolFlagTooLargeException : Exception
    {
        /// <summary>
        ///
        /// </summary>
        public override string Message => "Can't serialize more than 8 booleans in the flag";
    }
}
