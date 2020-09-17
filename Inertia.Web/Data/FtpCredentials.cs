using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Web
{
    /// <summary>
    /// Contains login informations for FTP access
    /// </summary>
    public class FtpCredential
    {
        #region Public variables

        /// <summary>
        /// Get the username of the login informations
        /// </summary>
        public readonly string Username;
        /// <summary>
        /// Get the password of the login informations
        /// </summary>
        public readonly string Password;
        /// <summary>
        /// Getthe host of the login informations
        /// </summary>
        public readonly string Host;
        /// <summary>
        /// Get the port of the login informations
        /// </summary>
        public readonly int Port;

        #endregion

        #region Internal variables

        internal string FtpHost;

        #endregion

        #region Constructors

        /// <summary>
        /// Instantiate a new instance of the class <see cref="FtpCredential"/>
        /// </summary>
        /// <param name="username">Username of the ftp login</param>
        /// <param name="password">Password of the ftp login</param>
        /// <param name="host">Host of the ftp login</param>
        public FtpCredential(string username, string password, string host) : this(username, password, host, 21)
        {

        }
        /// <summary>
        /// Instantiate a new instance of the class <see cref="FtpCredential"/>
        /// </summary>
        /// <param name="username">Username of the ftp login</param>
        /// <param name="password">Password of the ftp login</param>
        /// <param name="host">Host of the ftp login</param>
        /// <param name="port">Port of the ftp login</param>
        public FtpCredential(string username, string password, string host, int port)
        {
            Username = username;
            Password = password;
            Host = host;
            Port = port;
            FtpHost = host;

            if (!FtpHost.EndsWith("/"))
                FtpHost += "/";
        }

        #endregion

        /// <summary>
        /// Transform a <see cref="FtpCredential"/> instance to an <see cref="NetworkCredential"/> instance
        /// </summary>
        /// <param name="credentials">Instance to transform</param>
        public static implicit operator NetworkCredential(FtpCredential credentials)
        {
            return new NetworkCredential(credentials.Username, credentials.Password);
        }
    }
}
