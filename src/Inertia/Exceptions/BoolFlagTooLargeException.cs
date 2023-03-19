using System;

namespace Inertia
{
    /// <summary>
    /// Exception thrown when flag size limit is reached
    /// </summary>
    public sealed class BoolFlagTooLargeException : Exception
    {
        public override string Message => "Can't serialize more than 8 booleans in a flag";
    }
}