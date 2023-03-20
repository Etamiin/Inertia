using System;

namespace Inertia.Network
{
    public class UnknownMessageException : Exception
    {
        public override string Message => $"Invalid NetworkMessage ({ _messageId })";

        private readonly ushort _messageId;

        public UnknownMessageException(ushort messageId)
        {
            _messageId = messageId;
        }
    }
}