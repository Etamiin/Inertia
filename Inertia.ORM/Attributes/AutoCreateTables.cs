using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.ORM
{
    /// <summary>
    /// Represent the state that will automatically create all tables in a <see cref="Database"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class AutoCreateTables : Attribute
    {
        #region Constructors

        /// <summary>
        /// Instantiate a new instance of the class <see cref="AutoCreateTables"/>
        /// </summary>
        public AutoCreateTables()
        {
        }

        #endregion
    }
}
