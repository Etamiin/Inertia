using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.ORM
{
    /// <summary>
    /// Represents the link between a table and a database
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class DatabaseAttach : Attribute
    {
        #region Public variables

        /// <summary>
        /// Database type attached to
        /// </summary>
        public readonly Type DatabaseType;
        /// <summary>
        /// Database name attached to
        /// </summary>
        public readonly string DatabaseName;

        #endregion

        #region Constructors

        /// <summary>
        /// Instantiate a new instance of the class <see cref="DatabaseAttach"/>
        /// </summary>
        /// <param name="databaseType">Database type to attach</param>
        public DatabaseAttach(Type databaseType)
        {
            if (databaseType.IsAbstract || !databaseType.IsSubclassOf(typeof(Database)))
                return;

            DatabaseType = databaseType;
        }
        /// <summary>
        /// Instantiate a new instance of the class <see cref="DatabaseAttach"/>
        /// </summary>
        /// <param name="databaseName">Database name to attach</param>
        public DatabaseAttach(string databaseName)
        {
            DatabaseName = databaseName;
        }
        
        #endregion
    
        internal Database GetDatabase()
        {
            if (DatabaseType != null)
                return SqlManager.GetDatabase(DatabaseType);
            else
                return SqlManager.GetDatabase(DatabaseName);
        }
    }
}
