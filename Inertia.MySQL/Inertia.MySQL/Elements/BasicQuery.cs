using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.MySQL
{
    public class BasicQuery
    {
        public readonly string table;
        public readonly MySqlCommand command;

        public string Conditions { get; private set; } = "WHERE ";
        public bool HasCondition { get; private set; }

        public int Limit = -1;
        public bool HasLimit
        {
            get
            {
                return Limit == -1 ? false : true;
            }
        }

        public BasicQuery(string table)
        {
            this.table = table;
            command = MysqlModule.Module.CreateCommand();

        }

        public BasicQuery AddCondition(string column, object value)
        {
            command?.Parameters.AddWithValue("@" + column, value);
            Conditions += column + "=" + "@" + column;

            HasCondition = true;

            return this;
        }

        public BasicQuery AddLmit(int limit)
        {
            this.Limit = limit;
            return this;
        }

        public virtual string GenerateQuery()
        {
            return string.Empty;
        }

        internal string CompleteQuery(string query)
        {
            if (command == null)
                return "-broken";

            if (HasCondition)
                query += " " + Conditions;

            if (HasLimit)
                query += " LIMIT " + Limit;

            query += ";";

            return query;
        }
    }
}
