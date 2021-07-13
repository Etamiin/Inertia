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
        /// <summary>
        /// Get the exception message
        /// </summary>
        public override string Message => "You can't create a new web request when a request already exist.";

        /// <summary>
        /// Instantiate a new instance of the class <see cref="WebModuleBusyException"/>
        /// </summary>
        public WebModuleBusyException()
        {
        }
    }
}
