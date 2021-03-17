using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Web
{
    /// <summary>
    /// Thrown when an uri or path is invalid
    /// </summary>
    public class EnqueueInvalidInformationsException : Exception
    {
        /// <summary>
        /// Get the exception message
        /// </summary>
        public override string Message => $"You enqueued wrong informations, please verify your URI or FILENAME before processing to enqueue (uri: { m_uri })";

        private readonly string m_uri;

        /// <summary>
        /// Instantiate a new instance of the class <see cref="EnqueueInvalidInformationsException"/>
        /// </summary>
        /// <param name="uri">Uri or path that caused the exception</param>
        public EnqueueInvalidInformationsException(string uri)
        {
            m_uri = uri;
        }
    }
}
