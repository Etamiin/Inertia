using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.ORM
{
    public class ConditionQuery : IDisposable
    {
        #region Internal variables

        internal MySqlCommand Command { get; private set; }

        #endregion

        #region Private variables

        private List<string> Conditions;
        private readonly int Limit;

        #endregion

        #region Constructors

        internal ConditionQuery(MySqlCommand command) : this(command, -1)
        {
        }
        internal ConditionQuery(MySqlCommand command, int limit)
        {
            Conditions = new List<string>();
            Command = command;
            Limit = limit;
        }

        #endregion

        public ConditionQuery AddCondition(string field, object value, ConditionType type = ConditionType.AND)
        {
            var _id = "@" + field + type.ToString();
            var _condition = field + "=" + _id;

            if (Conditions.Count > 0)
                _condition = type.ToString() + " " + _condition;

            Command.Parameters.AddWithValue(_id, value);
            Conditions.Add(_condition);

            return this;
        }

        private string GenerateConditionQuery()
        {
            string query = string.Empty;

            if (Conditions.Count > 0)
            {
                query += "WHERE";
                for (var i = 0; i < Conditions.Count; i++)
                    query += " " + Conditions[i];
            }

            if (Limit >= 0)
                query += (string.IsNullOrEmpty(query) ? string.Empty : " ") + " LIMIT " + Limit;

            return query;
        }
        internal void JoinQuery(ref string query)
        {
            var conditionQuery = GenerateConditionQuery();
            if (!string.IsNullOrEmpty(conditionQuery))
                query += " " + conditionQuery;
        }

        public void Dispose()
        {
            Command = null;
            Conditions.Clear();
            Conditions = null;
        }
    }
}
