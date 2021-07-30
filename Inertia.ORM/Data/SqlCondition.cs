using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inertia.ORM
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class SqlCondition : IDisposable
    {
        private static Dictionary<ConditionOperator, string> m_operators = new Dictionary<ConditionOperator, string>()
        {
            { ConditionOperator.Equal, "=" }, { ConditionOperator.NotEqual, "<>" }, { ConditionOperator.Greater, ">" },
            { ConditionOperator.GreaterOrEqual, ">=" }, { ConditionOperator.Less, "<" }, { ConditionOperator.LessOrEqual, "<=" },
            { ConditionOperator.NotGreater, "!>" }, { ConditionOperator.NotLess, "!<" }, { ConditionOperator.In, " IN " }
        };

        /// <summary>
        /// 
        /// </summary>
        public bool IsDisposed { get; private set; }

        internal int ParamIndex;

        private readonly StringBuilder m_builder;
        private StringBuilder m_orderBuilder;
        private int m_limit;
        private bool m_bracketInput;
        private Dictionary<string, object> m_params;

        /// <summary>
        /// Initialize a new instance of the class <see cref="SqlCondition"/>.
        /// </summary>
        public SqlCondition()
        {
            m_builder = new StringBuilder();
            m_params = new Dictionary<string, object>();
        }

        /// <summary>
        /// Open a new bracket in the query (ex: "(").
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public SqlCondition BeginBrackets(ConditionType type = ConditionType.AND)
        {
            if (m_builder.Length > 0) m_builder.Append($" { type.ToString().ToUpper() } ");

            m_builder.Append("(");
            m_bracketInput = true;

            return this;
        }
        /// <summary>
        /// End a bracket in the query (ex: ")").
        /// </summary>
        /// <returns></returns>
        public SqlCondition EndBrackets()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(SqlCondition));

            m_builder.Append(")");
            m_bracketInput = false;

            return this;
        }
        /// <summary>
        /// Add a new condition with specified parameters.
        /// </summary>
        /// <param name="fieldName">Field name to check</param>
        /// <param name="value"></param>
        /// <param name="conditionOperator">SQL operator</param>
        /// <param name="type">SQL condition type</param>
        /// <returns></returns>
        public SqlCondition Add(string fieldName, object value, ConditionOperator conditionOperator, ConditionType type = ConditionType.AND)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(SqlCondition));

            if (value == null)
            {
                if (conditionOperator == ConditionOperator.Like)
                    return this;
                
                if (conditionOperator != ConditionOperator.Equal && conditionOperator != ConditionOperator.NotEqual)
                    conditionOperator = ConditionOperator.Equal;
            }

            if (m_builder.Length > 0 && (m_builder[m_builder.Length - 1] != '(' && m_builder[m_builder.Length - 1] != ')'))
                m_builder.Append($" { type } ");

            m_builder.Append(fieldName);

            if (value == null)
            {
                if (conditionOperator == ConditionOperator.Equal) m_builder.Append(" IS NULL");
                else if (conditionOperator == ConditionOperator.NotEqual) m_builder.Append(" IS NOT NULL");
            }
            else if (conditionOperator == ConditionOperator.Like) m_builder.Append($" LIKE { value }");
            else m_builder.Append(m_operators[conditionOperator]);

            if (conditionOperator != ConditionOperator.In)
            {
                var paramName = GetNextParamName();

                m_builder.Append(paramName);
                m_params.Add(paramName, value);
            }
            else AddIn();

            return this;

            void AddIn()
            {
                var inBuilder = new StringBuilder();
                if (value is IEnumerable<object> inList)
                {
                    for (var i = 0; i < inList.Count(); i++)
                    {
                        if (i > 0) inBuilder.Append(",");
                        AddInElement(inList.ElementAt(i));
                    }
                }
                else AddInElement(value);

                m_builder.Append(inBuilder.ToString());
                
                void AddInElement(object element)
                {
                    if (element.GetType().IsValueType) inBuilder.Append(element.ToString());
                    else inBuilder.Append($"\"{ element }\"");
                }
            }
        }
        /// <summary>
        /// Add a SQL "BETWEEN" condition with specified parameters.
        /// </summary>
        /// <param name="fieldName">Field name to check</param>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <param name="type">SQL condition type</param>
        /// <returns></returns>
        public SqlCondition AddBetween(string fieldName, object value1, object value2, ConditionType type = ConditionType.AND)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(SqlCondition));

            if (m_builder.Length > 0 && (m_builder[m_builder.Length - 1] != '(' && m_builder[m_builder.Length - 1] != ')'))
                m_builder.Append($" { type } ");

            var pn1 = GetNextParamName();
            var pn2 = GetNextParamName();
            
            m_builder.Append($"{ fieldName } BETWEEN { pn1 } AND { pn2 }");
            m_params.Add(pn1, value1);
            m_params.Add(pn2, value2);

            return this;
        }
        /// <summary>
        /// Add a custom string condition (ex: "id >= 2 OR value >= 5") to the current instance.
        /// </summary>
        /// <param name="strQuery"></param>
        /// <param name="type">SQL condition type</param>
        /// <returns></returns>
        public SqlCondition AddStringPattern(string strQuery, ConditionType type = ConditionType.AND)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(SqlCondition));

            if (m_builder.Length > 0 && (m_builder[m_builder.Length - 1] != '(' && m_builder[m_builder.Length - 1] != ')'))
                m_builder.Append($" { type } ");

            m_builder.Append(strQuery);
            return this;
        }
        
        /// <summary>
        /// Add a SQL "ORDER BY" condition by ascending.
        /// </summary>
        /// <param name="columns">Columns to order</param>
        /// <returns></returns>
        public SqlCondition OrderAscending(params string[] columns)
        {
            OrderBy("ASC", columns);
            return this;
        }
        /// <summary>
        /// Add a SQL "ORDER BY" condition by descending.
        /// </summary>
        /// <param name="columns">Columns to order</param>
        /// <returns></returns>
        public SqlCondition OrderDescending(params string[] columns)
        {
            OrderBy("DESC", columns);
            return this;
        }
        /// <summary>
        /// Add a SQL "LIMIT" condition.
        /// </summary>
        /// <param name="limit"></param>
        /// <returns></returns>
        public SqlCondition Limit(int limit)
        {
            m_limit = limit;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            if (IsDisposed)
                return;

            m_params.Clear();
            IsDisposed = true;
        }

        internal void ApplyToCmd(MySqlCommand command)
        {
            foreach (var param in m_params)
                command.Parameters.AddWithValue(param.Key, param.Value);

            command.Prepare();
        }
        internal string GetQuery()
        {
            if (m_bracketInput) EndBrackets();

            return 
                $"{ (m_builder.Length > 0 ? "WHERE " : string.Empty)}{ m_builder }" +
                $"{ (m_orderBuilder != null ? m_orderBuilder.ToString() : string.Empty) }" +
                $"{ (m_limit > 0 ? $" LIMIT { m_limit }" : string.Empty)}";
        }

        private void OrderBy(string type, params string[] columns)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(SqlCondition));

            if (m_orderBuilder == null) m_orderBuilder = new StringBuilder("ORDER BY ");
            else m_orderBuilder.Append(", ");

            m_orderBuilder.Append($"{ string.Join(",", columns) } { type }");
        }
        private string GetNextParamName()
        {
            return $"@{ ParamIndex++ }";
        }
    }
}
