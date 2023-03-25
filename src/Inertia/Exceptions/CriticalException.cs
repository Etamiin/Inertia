using System;

namespace Inertia
{
    public sealed class CriticalException : Exception
    {
        public override string Message => $"{_message}{(_ex != null ? $": {_ex}" : string.Empty)}";
        public Exception Exception => _ex;
        
        private readonly string _message;
        private readonly Exception? _ex;

        public CriticalException(string message) : this(message, null)
        {
        }
        public CriticalException(string message, Exception? ex)
        {
            _message = message;
            _ex = ex;
        }
    }
}
