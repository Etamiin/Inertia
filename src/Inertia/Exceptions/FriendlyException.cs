using System;
using System.Collections.Generic;
using System.Text;

namespace Inertia
{
    public class FriendlyException : Exception
    {
        public override string Message => _message;

        private string _message;

        public FriendlyException(string message)
        {
            _message = message;
        }
    }
}
