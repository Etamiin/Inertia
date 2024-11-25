using System;

namespace Inertia.Network
{
    public class InvalidNetworkProtocolException : Exception
    {
        public InvalidNetworkProtocolException(Type expectedType) : base($"Invalid {nameof(NetworkProtocol)} defined, expected protocol type: '{expectedType.Name}'.")
        {
        }
    }
}
