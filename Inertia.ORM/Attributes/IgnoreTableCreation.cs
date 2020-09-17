using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.ORM
{
    /// <summary>
    /// Define for a table that its creation is to be ignored
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class IgnoreTableCreation : Attribute
    {
        #region Constructors

        /// <summary>
        /// Instantiate a new instance of the class <see cref="IgnoreTableCreation"/>
        /// </summary>
        public IgnoreTableCreation()
        {
        }

        #endregion
    }
}
