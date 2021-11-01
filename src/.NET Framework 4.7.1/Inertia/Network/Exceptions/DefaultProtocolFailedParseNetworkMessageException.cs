using System;

namespace Inertia.Network
{
    public class DefaultProtocolFailedParseNetworkMessageException : Exception
    {
        public override string Message => _exMessage;

        private readonly string _exMessage;

        /// <summary>
        /// Initialize a new instance of the class <see cref="DefaultProtocolFailedParseNetworkMessageException"/>
        /// </summary>
        /// <param name="exMessage"></param>
        public DefaultProtocolFailedParseNetworkMessageException(string exMessage)
        {
            _exMessage = exMessage;
        }
    }
}
