using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.ORM
{
    /// <summary>
    /// Define for a <see cref="Database"/> that is ignored at the initialization
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class IgnoreDatabase : Attribute
    {
        #region Constructors

        /// <summary>
        /// Instantiate a new instance of the class <see cref="IgnoreDatabase"/>
        /// </summary>
        public IgnoreDatabase()
        {
        }

        #endregion
    }
}
