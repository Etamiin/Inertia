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
        #region Public variables

        /// <summary>
        /// Message of the exception
        /// </summary>
        public override string Message => GetMessage();
        /// <summary>
        /// Unknown received MessageID
        /// </summary>
        public readonly uint PacketId;

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageType"></param>
        public UnknownMessageException(uint packetId)
        {
            PacketId = packetId;
        }

        #endregion

        private string GetMessage()
        {
            return "Invalid Message ID(" + PacketId + ")";
        }
    }
}
