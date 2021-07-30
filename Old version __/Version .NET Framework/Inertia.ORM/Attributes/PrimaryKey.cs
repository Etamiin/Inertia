using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.ORM
{
    /// <summary>
    /// Represent a primarykey field in a table
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class PrimaryKey : Attribute
    {
        #region Public variables

        /// <summary>
        /// The field is set as auto increment if true
        /// </summary>
        public readonly bool AutoIncrement;

        #endregion

        #region Constructors

        /// <summary>
        /// Instantiate a new instance of the class <see cref="PrimaryKey"/>
        /// </summary>
        /// <param name="autoIncrement">Set the auto increment state of the primary key field</param>
        public PrimaryKey(bool autoIncrement)
        {
            AutoIncrement = autoIncrement;
        }

        #endregion
    }
}
