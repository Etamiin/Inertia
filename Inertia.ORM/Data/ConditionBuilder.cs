using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inertia.Internal;
using MySql.Data.MySqlClient;

namespace Inertia.ORM
{
    /// <summary>
    /// Represent the condition query builder
    /// </summary>
    public class ConditionBuilder : IDisposable
    {
        #region Internal variables

        internal MySqlCommand Command;
        internal int Length
        {
            get
            {
                return m_builder.Length;
            }
        }

        #endregion;

        #region Private variables

        private StringBuilder m_builder;
        private int m_limit = -1;

        #endregion

        #region Constructors

        /// <summary>
        /// Instantiate a new instance of the class <see cref="ConditionBuilder"/>
        /// </summary>
        /// <param name="db"><see cref="Database"/> attached to the <see cref="ConditionBuilder"/></param>
        public ConditionBuilder(Database db)
        {
            if (db == null)
                throw new NullReferenceException();

            Command = db.CreateCommand();
            m_builder = new StringBuilder();
        }
        /// <summary>
        /// Instantiate a new instance of the class <see cref="ConditionBuilder"/>
        /// </summary>
        /// <param name="table"><see cref="Table"/> attached to the <see cref="ConditionBuilder"/></param>
        public ConditionBuilder(Table table) : this(table.Database)
        {
        }

        #endregion

        /// <summary>
        /// Clear the current instance
        /// </summary>
        public void Clear()
        {
            Command.Parameters.Clear();
            m_builder.Clear();
        }

        /// <summary>
        /// Open brackets in the condition query
        /// </summary>
        /// <param name="type">Additional <see cref="ConditionType"/> if allowed</param>
        /// <returns>The current instance</returns>
        public ConditionBuilder OpenBrackets(ConditionType type = ConditionType.And)
        {
            if (m_builder.Length > 0)
                m_builder.Append(" " + type.ToString().ToUpper() + " ");

            m_builder.Append("(");
            return this;
        }
        /// <summary>
        /// Close brackets in the condition query
        /// </summary>
        /// <returns>The current instance</returns>
        public ConditionBuilder CloseBrackets()
        {
            m_builder.Append(")");
            return this;
        }
        /// <summary>
        /// Add condition to the query
        /// </summary>
        /// <param name="fieldName">Field name</param>
        /// <param name="value">Value of the condition</param>
        /// <param name="conditionOperator"><see cref="ConditionOperator"/> for the condition</param>
        /// <param name="type">Additional <see cref="ConditionType"/> to add if allowed</param>
        /// <returns>The current instance</returns>
        public ConditionBuilder AddCondition(string fieldName, object value, ConditionOperator conditionOperator, ConditionType type = ConditionType.And)
        {
            if (m_builder.Length > 0)
            {
                var lastChar = m_builder[m_builder.Length - 1];
                if (lastChar != '(')
                    m_builder.Append(" " + type.ToString().ToUpper() + " ");
            }

            m_builder.Append(fieldName);

            if (value == null && (conditionOperator != ConditionOperator.Equal && conditionOperator != ConditionOperator.NotEqual))
                conditionOperator = ConditionOperator.Equal;

            switch (conditionOperator)
            {
                case ConditionOperator.Equal:
                    if (value == null)
                        m_builder.Append(" IS NULL");
                    else
                        m_builder.Append("=");
                    break;
                case ConditionOperator.NotEqual:
                    if (value == null)
                        m_builder.Append(" IS NOT NULL");
                    else
                        m_builder.Append("<>");
                    break;
                case ConditionOperator.Greater:
                    m_builder.Append(">");
                    break;
                case ConditionOperator.GreaterOrEqual:
                    m_builder.Append(">=");
                    break;
                case ConditionOperator.Less:
                    m_builder.Append("<");
                    break;
                case ConditionOperator.LessOrEqual:
                    m_builder.Append("<=");
                    break;
                case ConditionOperator.NotGreater:
                    m_builder.Append("!>");
                    break;
                case ConditionOperator.NotLess:
                    m_builder.Append("!<");
                    break;
                case ConditionOperator.In:
                    m_builder.Append(" IN ");
                    break;
            }

            if (conditionOperator == ConditionOperator.In)
            {
                var strValue = string.Empty;
                if (value is IEnumerable<object> inList)
                {
                    for (var i = 0; i < inList.Count(); i++)
                    {
                        var element = inList.ElementAt(i);
                        AddInElement(element);

                        if (i < inList.Count() - 1)
                            strValue += ",";
                    }
                }
                else
                    AddInElement(value);

                m_builder.Append("(" + strValue + ")");

                void AddInElement(object element)
                {
                    if (element.GetType().IsValueType)
                        strValue += element.ToString();
                    else
                        strValue += '"' + element.ToString() + '"';
                }
            }
            else if (value != null)
            {
                var paramName = OrmHelper.GetRandomName(7);

                m_builder.Append(paramName);
                Command.Parameters.AddWithValue(paramName, value);
            }

            return this;
        }
        /// <summary>
        /// Set the "LIMIT" state of the query
        /// </summary>
        /// <param name="limit"></param>
        /// <returns>The current instance</returns>
        public ConditionBuilder SetLimit(int limit)
        {
            m_limit = limit;
            return this;
        }

        /// <summary>
        /// Dispose the current instance
        /// </summary>
        public void Dispose()
        {
            Command.Dispose();
            m_builder = null;
            Command = null;
        }

        internal string GetQuery()
        {
            var query = m_builder.ToString();
            if (!query.StartsWith("WHERE BINARY "))
                query = "WHERE BINARY " + query;

            if (m_limit >= 0)
                query += " LIMIT " + m_limit;

            return query;
        }
    }
}
