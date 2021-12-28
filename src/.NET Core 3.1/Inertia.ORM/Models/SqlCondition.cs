using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inertia.ORM
{
    public sealed class SqlCondition : IDisposable
    {
        private static Dictionary<ConditionOperator, string> _operators = new Dictionary<ConditionOperator, string>
        {
            { ConditionOperator.Equal, "=" }, { ConditionOperator.NotEqual, "<>" }, { ConditionOperator.Greater, ">" },
            { ConditionOperator.GreaterOrEqual, ">=" }, { ConditionOperator.Less, "<" }, { ConditionOperator.LessOrEqual, "<=" },
            { ConditionOperator.NotGreater, "!>" }, { ConditionOperator.NotLess, "!<" }, { ConditionOperator.In, " IN " }
        };

        public bool IsDisposed { get; private set; }

        internal int ParamIndex;

        private readonly StringBuilder _builder;
        private readonly Dictionary<string, object> _params;
        private StringBuilder _orderBuilder;
        private int _limit;
        private bool _bracketInput;

        /// <summary>
        /// Initialize a new instance of the class <see cref="SqlCondition"/>.
        /// </summary>
        public SqlCondition()
        {
            _builder = new StringBuilder();
            _params = new Dictionary<string, object>();
        }

        /// <summary>
        /// Open a new bracket in the query (ex: "(").
        /// </summary>
        /// <returns></returns>
        public SqlCondition BeginBrackets()
        {
            return BeginBrackets(ConditionType.AND);
        }
        /// <summary>
        /// Open a new bracket in the query (ex: "(").
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public SqlCondition BeginBrackets(ConditionType type)
        {
            if (_builder.Length > 0)
            {
                _builder.Append($" { type.ToString().ToUpper() } ");
            }

            _builder.Append("(");
            _bracketInput = true;

            return this;
        }
        /// <summary>
        /// End a bracket in the query (ex: ")").
        /// </summary>
        /// <returns></returns>
        public SqlCondition EndBrackets()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(SqlCondition));
            }

            _builder.Append(")");
            _bracketInput = false;

            return this;
        }
        /// <summary>
        /// Add a new condition with specified parameters.
        /// </summary>
        /// <param name="fieldName">Field name to check</param>
        /// <param name="value"></param>
        /// <param name="conditionOperator">SQL operator</param>
        /// <returns></returns>
        public SqlCondition Add(string fieldName, object value, ConditionOperator conditionOperator)
        {
            return Add(fieldName, value, conditionOperator, ConditionType.AND);
        }
        /// <summary>
        /// Add a new condition with specified parameters.
        /// </summary>
        /// <param name="fieldName">Field name to check</param>
        /// <param name="value"></param>
        /// <param name="conditionOperator">SQL operator</param>
        /// <param name="type">SQL condition type</param>
        /// <returns></returns>
        public SqlCondition Add(string fieldName, object value, ConditionOperator conditionOperator, ConditionType type)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(SqlCondition));
            }

            if (value == null)
            {
                if (conditionOperator == ConditionOperator.Like)
                {
                    return this;
                }
                
                if (conditionOperator != ConditionOperator.Equal && conditionOperator != ConditionOperator.NotEqual)
                {
                    conditionOperator = ConditionOperator.Equal;
                }
            }

            if (_builder.Length > 0 && (_builder[_builder.Length - 1] != '(' && _builder[_builder.Length - 1] != ')'))
            {
                _builder.Append($" { type } ");
            }

            _builder.Append(fieldName);

            if (value == null)
            {
                if (conditionOperator == ConditionOperator.Equal)
                {
                    _builder.Append(" IS NULL");
                }
                else if (conditionOperator == ConditionOperator.NotEqual)
                {
                    _builder.Append(" IS NOT NULL");
                }
            }
            else if (conditionOperator == ConditionOperator.Like)
            {
                _builder.Append($" LIKE { value }");
            }
            else
            {
                _builder.Append(_operators[conditionOperator]);
            }

            if (conditionOperator != ConditionOperator.In)
            {
                var paramName = GetNextParamName();

                _builder.Append(paramName);
                _params.Add(paramName, value);
            }
            else
            {
                AddIn();
            }

            return this;

            void AddIn()
            {
                var inBuilder = new StringBuilder();
                if (value is IEnumerable<object> inList)
                {
                    for (var i = 0; i < inList.Count(); i++)
                    {
                        if (i > 0)
                        {
                            inBuilder.Append(",");
                        }

                        AddInElement(inList.ElementAt(i));
                    }
                }
                else
                {
                    AddInElement(value);
                }

                _builder.Append(inBuilder.ToString());
                
                void AddInElement(object element)
                {
                    if (element.GetType().IsValueType)
                    {
                        inBuilder.Append(element.ToString());
                    }
                    else
                    {
                        inBuilder.Append($"\"{ element }\"");
                    }
                }
            }
        }
        /// <summary>
        /// Add a SQL "BETWEEN" condition with specified parameters.
        /// </summary>
        /// <param name="fieldName">Field name to check</param>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <returns></returns>
        public SqlCondition AddBetween(string fieldName, object value1, object value2)
        {
            return AddBetween(fieldName, value1, value2, ConditionType.AND);
        }
        /// <summary>
        /// Add a SQL "BETWEEN" condition with specified parameters.
        /// </summary>
        /// <param name="fieldName">Field name to check</param>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <param name="type">SQL condition type</param>
        /// <returns></returns>
        public SqlCondition AddBetween(string fieldName, object value1, object value2, ConditionType type)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(SqlCondition));
            }

            if (_builder.Length > 0 && (_builder[_builder.Length - 1] != '(' && _builder[_builder.Length - 1] != ')'))
            {
                _builder.Append($" { type } ");
            }

            var pn1 = GetNextParamName();
            var pn2 = GetNextParamName();

            _builder.Append($"{ fieldName } BETWEEN { pn1 } AND { pn2 }");
            _params.Add(pn1, value1);
            _params.Add(pn2, value2);

            return this;
        }
        /// <summary>
        /// Add a custom string condition (ex: "id >= 2 OR value >= 5") to the current instance.
        /// </summary>
        /// <param name="strQuery"></param>
        /// <returns></returns>
        public SqlCondition AddStringPattern(string strQuery)
        {
            return AddStringPattern(strQuery, ConditionType.AND);
        }
        /// <summary>
        /// Add a custom string condition (ex: "id >= 2 OR value >= 5") to the current instance.
        /// </summary>
        /// <param name="strQuery"></param>
        /// <param name="type">SQL condition type</param>
        /// <returns></returns>
        public SqlCondition AddStringPattern(string strQuery, ConditionType type)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(SqlCondition));
            }

            if (_builder.Length > 0 && (_builder[_builder.Length - 1] != '(' && _builder[_builder.Length - 1] != ')'))
            {
                _builder.Append($" { type } ");
            }

            _builder.Append(strQuery);
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
        /// <param name="limitValue"></param>
        /// <returns></returns>
        public SqlCondition Limit(int limitValue)
        {
            _limit = limitValue;
            return this;
        }

        internal void ApplyToCmd(MySqlCommand command)
        {
            foreach (var param in _params)
            {
                command.Parameters.AddWithValue(param.Key, param.Value);
            }
        }
        internal string GetQuery()
        {
            if (_bracketInput)
            {
                EndBrackets();
            }

            return
                $"{ (_builder.Length > 0 ? "WHERE " : string.Empty)}{ _builder }" +
                $"{ (_orderBuilder != null ? _orderBuilder.ToString() : string.Empty) }" +
                $"{ (_limit > 0 ? $" LIMIT { _limit }" : string.Empty)}";
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                _params.Clear();
                IsDisposed = true;
            }
        }

        private void OrderBy(string type, params string[] columns)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(SqlCondition));
            }

            if (_orderBuilder == null)
            {
                _orderBuilder = new StringBuilder("ORDER BY ");
            }
            else
            {
                _orderBuilder.Append(", ");
            }

            _orderBuilder.Append($"{ string.Join(",", columns) } { type }");
        }
        private string GetNextParamName()
        {
            return $"@{ ParamIndex++ }";
        }
    }
}
