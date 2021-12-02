using System;

namespace Inertia.ORM
{
    /// <summary>
    /// Throw when <see cref="SqlManager"/> failed to initialize
    /// </summary>
    public class InitializationFailedException : Exception
    {
<<<<<<< HEAD
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
=======
        public override string Message => _exMessage;

        private readonly string _exMessage;

        public InitializationFailedException(string exMsg)
        {
            _exMessage = exMsg;
>>>>>>> premaster
        }
    }
}
