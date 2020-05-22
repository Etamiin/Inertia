using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.ORM
{
    /// <summary>
    /// Throw when a <see cref="Table"/> class don't have any fields
    /// </summary>
    public class NoFieldsException : Exception
    {
        #region Public variables

        /// <summary>
        /// Message of the exception
        /// </summary>
        public override string Message => GetMessage();

        #endregion

        #region Private variables

        private Table m_table;

        #endregion

        #region Constructors

        /// <summary>
        /// Instantiante a new instance of the class <see cref="NoFieldsException"/>
        /// </summary>
        /// <param name="table"></param>
        public NoFieldsException(Table table)
        {
            m_table = table;
        }

        #endregion

        private string GetMessage()
        {
            return "Can't use table '" + m_table.Name + "' no fields founded or modified";
        }
    }
}
