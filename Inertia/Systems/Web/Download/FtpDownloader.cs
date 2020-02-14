using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Web
{
    public class FtpDownloader : InertiaDownloaderModule
    {
        #region Public variables

        public readonly FtpCredential Credentials;

        #endregion

        #region Constructors

        public FtpDownloader(FtpCredential credentials) : base(credentials)
        {
            Credentials = credentials;
        }

        #endregion

    }
}
