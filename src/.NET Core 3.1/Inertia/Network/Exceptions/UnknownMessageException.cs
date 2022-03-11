using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Network
{
    /// <summary>
    /// Thrown when using a <see cref="Type"/> that isn't a subclass of <see cref="NetworkMessage"/>
    /// </summary>
    public class UnknownMessageException : Exception
    {
        public override string Message => $"Invalid NetworkMessage (ID:{ _messageId })";

        private readonly ushort _messageId;

        public UnknownMessageException(ushort messageId)
        {
            _messageId = messageId;
        }
    }
}