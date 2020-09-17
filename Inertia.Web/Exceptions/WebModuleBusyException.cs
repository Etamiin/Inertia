using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Web
{
    /// <summary>
    /// Thrown when using a web class module that is busy
    /// </summary>
    public class WebModuleBusyException : Exception
    {
        #region Public variables

        /// <summary>
        /// Get the exception message
        /// </summary>
        public override string Message => GetMessage();

        #endregion

        #region Constructors

        /// <summary>
        /// Instantiate a new instance of the class <see cref="WebModuleBusyException"/>
        /// </summary>
        public WebModuleBusyException()
        {
        }

        #endregion

        private string GetMessage()
        {
            return
                "You can't create a new web request when a request already exist.";
        }
    }
}
