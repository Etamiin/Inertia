using System;

namespace Inertia.Network
{
    public class UnknownMessageException : Exception
    {
        public UnknownMessageException(ushort messageId) : base($"Invalid {nameof(NetworkMessage)} (ID: {messageId})")
        {
        }
    }
}