using System;
using System.Collections.Generic;
using System.Text;

namespace Inertia
{
    public class CriticalException : Exception
    {
        public override string Message => $"{_message}: {_ex}";
        public Exception Exception => _ex;
        
        private string _message;
        private Exception _ex;

        public CriticalException(string message, Exception ex)
        {
            _message = message;
            _ex = ex;
        }
    }
}
