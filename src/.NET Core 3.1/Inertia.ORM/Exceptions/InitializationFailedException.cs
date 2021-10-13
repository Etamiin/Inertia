using System;

namespace Inertia.ORM
{
    /// <summary>
    /// Throw when <see cref="SqlManager"/> failed to initialize
    /// </summary>
    public class InitializationFailedException : Exception
    {
        /// <summary>
        ///
        /// </summary>
        public override string Message => ExMessage;

        private readonly string ExMessage;

        /// <summary>
        /// Instantiate a new instance of the class <see cref="InitializationFailedException"/>
        /// </summary>
        /// <param name="exMsg"></param>
        public InitializationFailedException(string exMsg)
        {
            ExMessage = exMsg;
        }
    }
}
