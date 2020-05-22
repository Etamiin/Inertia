using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia
{
    /// <summary>
    /// Exception thrown when a file storage use a bad password
    /// </summary>
    public class InvalidPasswordException : Exception
    {
        #region Public variables

        /// <summary>
        /// Get the message of the current exception
        /// </summary>
        public override string Message => GetMessage();

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize a new instance of the class <see cref="InvalidPasswordException"/>
        /// </summary>
        public InvalidPasswordException()
        {
        }

        #endregion

        private string GetMessage()
        {
            return "The specified password isn't valid.";
        }
    }
}
