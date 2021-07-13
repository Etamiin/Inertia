using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.ORM
{
    /// <summary>
    /// Throw when failed to connect to a <see cref="Database"/>
    /// </summary>
    public class DatabaseConnectionFailedException : Exception
    {
        #region Public variables

        /// <summary>
        /// Message of the exception
        /// </summary>
        public override string Message => GetMessage();

        /// <summary>
        /// Database that throwed the exception
        /// </summary>
        public Database Database { get; private set; }
        public Exception Exception { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Instantiante a new instance of the class <see cref="DatabaseConnectionFailedException"/>
        /// </summary>
        /// <param name="database"></param>
        public DatabaseConnectionFailedException(Database database, Exception ex)
        {
            Database = database;

        }

        #endregion

        private string GetMessage()
        {
            return "Can't connect to database '" + Database.Name + "'. Invalid credentials?";
        }
    }
}
