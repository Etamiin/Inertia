using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.ORM
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class PrimaryKeyField : Attribute
    {
        #region Public variables

        public readonly bool IsAutoIncrement;

        #endregion

        #region Constructors

        public PrimaryKeyField(bool autoIncrement)
        {
            IsAutoIncrement = autoIncrement;
        }

        #endregion

    }
}
