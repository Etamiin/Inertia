using System;

namespace Inertia
{
    /// <summary>
    /// Exception thrown when flag size limit is reached
    /// </summary>
    public class BoolFlagTooLargeException : Exception
    {
<<<<<<< HEAD
<<<<<<< HEAD
        /// <summary>
        ///
        /// </summary>
=======
>>>>>>> 9bfc85f6784b254a10c65f104446a83c8b195c40
=======
>>>>>>> premaster
        public override string Message => "Can't serialize more than 8 booleans in the flag";
    }
}
