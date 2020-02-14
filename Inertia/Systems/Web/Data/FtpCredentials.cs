using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Inertia.Internal;

namespace Inertia.Web
{
    public class FtpCredential : IDisposable
    {
        #region Public variables

        public string Username { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }

        #endregion

        #region Internal variables

        internal string CompleteHost;

        #endregion

        #region Constructors

        public FtpCredential(string username, string password, string host)
        {
            Username = username;
            Password = password;
            Host = host;
            CompleteHost = host;

            StringConventionNormalizer.NormalizeFtpHostAdressUri(ref CompleteHost);
        }

        #endregion

        public static implicit operator NetworkCredential(FtpCredential credentials)
        {
            return new NetworkCredential(credentials.Username, credentials.Password);
        }

        public void Dispose()
        {
            Username = null;
            Password = null;
            Host = null;
        }
    }
}
