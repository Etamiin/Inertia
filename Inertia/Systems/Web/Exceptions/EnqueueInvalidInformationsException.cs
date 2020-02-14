using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Web
{
    public class EnqueueInvalidInformationsException : Exception
    {
        #region Public variables

        public override string Message => GetMessage();

        #endregion

        #region Private variables

        private readonly string uri;

        #endregion

        #region Constructors

        public EnqueueInvalidInformationsException(string uri)
        {
            this.uri = uri;
        }

        #endregion

        private string GetMessage()
        {
            return
                "You enqueued wrong informations, please verify your URI or FILENAME before processing to enqueue {uri: " + uri + "}";
        }
    }
}
