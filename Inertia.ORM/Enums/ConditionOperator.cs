using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.ORM
{
    /// <summary>
    /// Conditonal operators
    /// </summary>
    public enum ConditionOperator
    {
        /// <summary>
        /// "=" operator
        /// </summary>
        Equal,
        /// <summary>
        /// ">" operator
        /// </summary>
        Greater,
        /// <summary>
        /// <![CDATA["<"]]> operator
        /// </summary>
        Less,
        /// <summary>
        /// ">=" operator
        /// </summary>
        GreaterOrEqual,
        /// <summary>
        /// <![CDATA["<="]]> operator operator
        /// </summary>
        LessOrEqual,
        /// <summary>
        /// <![CDATA["<>"]]> operator operator
        /// </summary>
        Different,
        /// <summary>
        /// "IN" operator
        /// </summary>
        In
    }
}
