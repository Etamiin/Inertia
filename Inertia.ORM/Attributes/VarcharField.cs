using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.ORM
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class VarcharField : Attribute
    {
        #region Public variables

        public readonly int Length;

        #endregion

        #region Constructors

        public VarcharField(int length)
        {
            Length = length;
        }

        #endregion

    }
}
