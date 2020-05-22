using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.ORM
{
    /// <summary>
    /// Throw when a <see cref="DatabaseAttach"/> attribute has a not existing Database
    /// </summary>
    public class InvalidDatabaseAttachException : Exception
    {
        #region Public variables

        /// <summary>
        /// Message of the exception
        /// </summary>
        public override string Message => GetMessage();
        /// <summary>
        /// Database name to attach
        /// </summary>
        public readonly string DatabaseName;

        #endregion

        #region Constructors

        /// <summary>
        /// Instantiate a new instance of the class <see cref="InvalidDatabaseAttachException"/>
        /// </summary>
        /// <param name="databaseName">Name of the database to attach</param>
        public InvalidDatabaseAttachException(string databaseName)
        {
            DatabaseName = databaseName;
        }

        #endregion

        private string GetMessage()
        {
            return "No database class used the name '" + DatabaseName + "' finded.";
        }
    }
}
