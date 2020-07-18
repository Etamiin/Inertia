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
        public readonly Type DatabaseType;

        #endregion

        #region Constructors

        /// <summary>
        /// Instantiate a new instance of the class <see cref="InvalidDatabaseAttachException"/>
        /// </summary>
        /// <param name="databaseType">Type of the database to attach</param>
        public InvalidDatabaseAttachException(Type databaseType)
        {
            DatabaseType = databaseType;
        }

        #endregion

        private string GetMessage()
        {
            return "The type '" + DatabaseType.Name + "' isn't a " + nameof(Database) + " subclass.";
        }
    }
}
