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
        /// greater operator
        /// </summary>
        Greater,
        /// <summary>
        /// less operator
        /// </summary>
        Less,
        /// <summary>
        /// greater or equal operator
        /// </summary>
        GreaterOrEqual,
        /// <summary>
        /// less or equal operator
        /// </summary>
        LessOrEqual,
        /// <summary>
        /// not equal operator
        /// </summary>
        NotEqual,
        /// <summary>
        /// not greater operator
        /// </summary>
        NotGreater,
        /// <summary>
        /// not less operator
        /// </summary>
        NotLess,
        /// <summary>
        /// "IN" operator
        /// </summary>
        In,
        /// <summary>
        /// "LIKE" operator
        /// </summary>
        Like
    }
}
