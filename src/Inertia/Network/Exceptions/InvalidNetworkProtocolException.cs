using System;

namespace Inertia.Network
{
    public class InvalidNetworkProtocolException : Exception
    {
        public override string Message => $"Invalid {nameof(NetworkProtocol)} defined, expected protocol type: '{_expectedType.Name}'.";

        private Type _expectedType;

        public InvalidNetworkProtocolException(Type expectedType)
        {
            _expectedType = expectedType;
        }
    }
}
