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
    public class HookerInvalidTypeException : Exception
    {
        #region Public variables

        /// <summary>
        /// Message of the exception
        /// </summary>
        public override string Message => GetMessage();
        /// <summary>
        /// 
        /// </summary>
        public readonly Type SpecifiedType;

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageType"></param>
        public HookerInvalidTypeException(Type messageType)
        {
            SpecifiedType = messageType;
        }

        #endregion

        private string GetMessage()
        {
            return "Invalid used Type (" + SpecifiedType.Name + "), the specified type need to be a subclass of " + nameof(NetworkMessage);
        }
    }
}
