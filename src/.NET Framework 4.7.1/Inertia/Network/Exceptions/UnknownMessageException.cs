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
        public override string Message => $"Invalid Message ID({ _packetId })";

        private readonly uint _packetId;

        public UnknownMessageException(uint packetId)
        {
            _packetId = packetId;
        }
    }
}
