using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.ORM
{
    /// <summary>
    /// Represent a varchar field in a table
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class VarChar : Attribute
    {
        #region Public variables

        /// <summary>
        /// Max length of the varchar field
        /// </summary>
        public readonly int Length;

        #endregion

        #region Constructors

        /// <summary>
        /// Instantiate a new instance of the class <see cref="VarChar"/>
        /// </summary>
        /// <param name="length"></param>
        public VarChar(int length)
        {
            Length = length;
        }

        #endregion
    }
}
