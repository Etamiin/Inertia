using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.ORM
{
    /// <summary>
    /// Add a "NOT NULL" statement to the field
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class NotNull : Attribute
    {
        #region Public variables

        /// <summary>
        /// Unique statement
        /// </summary>
        public readonly bool Unique;

        #endregion

        #region Constructors

        /// <summary>
        /// Instantiate a new instance of the class <see cref="NotNull"/>
        /// </summary>
        /// <param name="unique">Add unique statement to the field ?</param>
        public NotNull(bool unique = false)
        {
            Unique = unique;
        }

        #endregion
    }
}
