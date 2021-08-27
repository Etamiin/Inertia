using System;

namespace Inertia.Network
{
    /// <summary>
    /// 
    /// </summary>
    public class DefaultProtocolFailedParseNetworkMessageException : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        public override string Message => _exMessage;

        private string _exMessage;

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
