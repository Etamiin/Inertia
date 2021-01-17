using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inertia.Internal;

namespace Inertia.ORM
{
    public class SqlCondition<T> where T : Table
    {
        private readonly SqlQuery<T> m_query;
        private readonly StringBuilder m_builder;
        private StringBuilder m_orderBuilder;
        private int m_limit;
        private bool m_inBrackets;

        internal SqlCondition(SqlQuery<T> query)
        {
            m_query = query;
            m_builder = new StringBuilder();
        }

        public SqlQuery<T> BackToQuery()
        {
            return m_query;
        }

        public SqlCondition<T> BeginBrackets(ConditionType type = ConditionType.And)
        {
            if (m_builder.Length > 0)
                m_builder.Append(" " + type.ToString().ToUpper() + " ");

            m_builder.Append("(");
            m_inBrackets = true;

            return this;
        }
        public SqlCondition<T> EndBrackets()
        {
            m_builder.Append(")");
            m_inBrackets = false;

            return this;
        }
        public SqlCondition<T> Add(string fieldName, object value, ConditionOperator conditionOperator, ConditionType type = ConditionType.And)
        {
            if (value == null && (conditionOperator != ConditionOperator.Equal && conditionOperator != ConditionOperator.NotEqual))
                conditionOperator = ConditionOperator.Equal;

            if (m_builder.Length > 0 && (m_builder[m_builder.Length - 1] != '(' && m_builder[m_builder.Length - 1] != ')'))
                m_builder.Append(" " + type.ToString().ToUpper() + " ");

            m_builder.Append(fieldName);

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
                    AddIn();
                    break;
                case ConditionOperator.Like:
                    m_builder.Append(" LIKE " + value.ToString());
                    break;
            }

            if (conditionOperator != ConditionOperator.In && conditionOperator != ConditionOperator.Like && value != null)
            {
                var paramName = SqlManager.GenerateRandomName();

                m_builder.Append(paramName);
                m_query.Command.Parameters.AddWithValue(paramName, value);
            }
            
            return this;

            void AddIn()
            {
                var strValue = string.Empty;
                if (value is IEnumerable<object> inList)
                {
                    for (var i = 0; i < inList.Count(); i++)
                    {
                        if (i > 0)
                            strValue += ",";

                        var element = inList.ElementAt(i);
                        AddInElement(element);
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
        }
        public SqlCondition<T> AddPattern(string pattern, ConditionType type = ConditionType.And)
        {
            if (m_builder.Length > 0 && (m_builder[m_builder.Length - 1] != '(' && m_builder[m_builder.Length - 1] != ')'))
                m_builder.Append(" " + type.ToString().ToUpper() + " ");

            m_builder.Append(pattern);

            return this;
        }

        public SqlCondition<T> OrderAscending(params string[] columns)
        {
            OrderBy("ASC", columns);
            return this;
        }
        public SqlCondition<T> OrderDescending(params string[] columns)
        {
            OrderBy("DESC", columns);
            return this;
        }

        public SqlCondition<T> Limit(int limit)
        {
            m_limit = limit;
            return this;
        }

        private void OrderBy(string type, params string[] columns)
        {
            if (m_orderBuilder == null)
            {
                m_orderBuilder = new StringBuilder("ORDER BY ");
            }
            else
                m_orderBuilder.Append(", ");

            for (var i = 0; i < columns.Length; i++)
            {
                if (i > 0)
                    m_orderBuilder.Append(",");

                m_orderBuilder.Append(columns[i]);
            }

            m_orderBuilder.Append(" " + type);
        }

        internal string GetQuery()
        {
            if (m_inBrackets)
                EndBrackets();

            return (m_builder.Length > 0 ? "WHERE "  : string.Empty) +
                m_builder.ToString() +
                (m_orderBuilder != null ? m_orderBuilder.ToString() : string.Empty) +
                (m_limit > 0 ? " LIMIT " + m_limit : string.Empty);
        }
    }
}
