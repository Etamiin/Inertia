using System;

namespace Inertia.ORM
{
    /// <summary>
    /// Throw when <see cref="SqlManager"/> failed to initialize
    /// </summary>
    public class InitializationFailedException : Exception
    {
        public override string Message => _exMessage;

        private readonly string _exMessage;

        public InitializationFailedException(string exMsg)
        {
            _exMessage = exMsg;
        }
    }
}
