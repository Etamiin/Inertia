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
        /// <summary>
        /// Message of the exception
        /// </summary>
        public override string Message => GetMessage();

        private readonly uint _packetId;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packetId"></param>
        public UnknownMessageException(uint packetId)
        {
            _packetId = packetId;
        }

        private string GetMessage()
        {
            return $"Invalid Message ID({ _packetId })";
        }
    }
}
