using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Web
{
    public class WebModuleBusyException : Exception
    {
        #region Public variables

        public override string Message => GetMessage();

        #endregion

        #region Constructors

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
