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
        /// Database name attached to
        /// </summary>
        public readonly string Database;

        #endregion

        #region Constructors

        /// <summary>
        /// Instantiate a new instance of the class <see cref="DatabaseAttach"/>
        /// </summary>
        /// <param name="database">Database name to attach</param>
        public DatabaseAttach(string database)
        {
            Database = database;
        }
        
        #endregion
    }
}
