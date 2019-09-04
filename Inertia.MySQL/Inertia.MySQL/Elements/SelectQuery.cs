using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.MySQL
{
    public class SelectQuery : BasicQuery
    {
        private string fields;

        public SelectQuery(string table) : base(table)
        {
        }

        public new SelectQuery AddCondition(string column, object value)
        {
            base.AddCondition(column, value);
            return this;
        }
        public new SelectQuery AddLmit(int limit)
        {
            base.AddLmit(limit);
            return this;
        }

        public SelectQuery AddFields(params string[] columns)
        {
            foreach (var column in columns) {
                if (!string.IsNullOrEmpty(fields))
                    fields += ",";
                fields += column;
            }

            return this;
        }
        
        public QueryResultSelector Execute()
        {
            if (command == null)
                return null;

            try
            {
                command.CommandText = GenerateQuery();
                var reader = command.ExecuteReader();

                return new QueryResultSelector(reader);
            }
            catch (Exception e) {
                InDebug.Error(e);
                return null;
            }
        }

        public override string GenerateQuery()
        {
            string query =
                "SELECT " +
                (string.IsNullOrEmpty(fields) ? "*" : fields) +
                " FROM `" + table + "`";

            return CompleteQuery(query);
        }

    }
}
