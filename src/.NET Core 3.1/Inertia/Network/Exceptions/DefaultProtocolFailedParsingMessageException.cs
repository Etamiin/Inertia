using System;

namespace Inertia.Network
{
    public class DefaultProtocolFailedParsingMessageException : Exception
    {
        public override string Message => _exMessage;

        private readonly string _exMessage;

        public DefaultProtocolFailedParsingMessageException(string exMessage)
        {
            _exMessage = exMessage;
        }
    }
}
