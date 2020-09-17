using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Network
{
    /// <summary>
    /// Thrown when sending more than <see cref="ushort.MaxValue"/> bytes from UDP protocol
    /// </summary>
    public class UserDatagramDataLengthLimitException : Exception
    {
        #region Public variables

        /// <summary>
        /// Message of the exception
        /// </summary>
        public override string Message => GetMessage();
        /// <summary>
        /// Length of bytes trying to sent
        /// </summary>
        public readonly int Length;

        #endregion

        #region Constructors

        /// <summary>
        /// Instantiate a new instance of the class <see cref="UserDatagramDataLengthLimitException"/>
        /// </summary>
        /// <param name="length"></param>
        public UserDatagramDataLengthLimitException(int length)
        {
            Length = length;
        }

        #endregion

        private string GetMessage()
        {
            return
                "You can't send more than " + ushort.MaxValue + " bytes from user datagram networking.\n" +
                "Trying to send " + Length + " bytes.";
        }
    }
}
