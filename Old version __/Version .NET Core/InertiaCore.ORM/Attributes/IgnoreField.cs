using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.ORM
{
    /// <summary>
    /// Define for a field that is ignored by the ORM system
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class IgnoreField : Attribute
    {
        #region Constructors

        /// <summary>
        /// Instantiate a new instance of the class <see cref="IgnoreField"/>
        /// </summary>
        public IgnoreField()
        {
        }

        #endregion
    }
}
