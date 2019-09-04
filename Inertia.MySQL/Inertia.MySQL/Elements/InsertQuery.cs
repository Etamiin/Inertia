using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.MySQL
{
    public class InsertQuery : BasicQuery
    {
        private string columns;
        private string values;

        public InsertQuery(string table) : base(table)
        {
        }

        public new InsertQuery AddCondition(string column, object value)
        {
            base.AddCondition(column, value);
            return this;
        }

        public InsertQuery AddValue(string column, object value)
        {
            command?.Parameters.AddWithValue("@" + column, value);

            if (!string.IsNullOrEmpty(columns))
                columns += ",";
            columns += column;

            if (!string.IsNullOrEmpty(values))
                values += ",";
            values += "@" + column;

            return this;
        }

        public long Execute()
        {
            if (command == null || string.IsNullOrEmpty(columns))
                return -1;

            try {
                command.CommandText = GenerateQuery();
                command.ExecuteNonQuery();

                return command.LastInsertedId;
            }
            catch (Exception e) {
                InDebug.Error(e);
                return 0;
            }
        }

        public override string GenerateQuery()
        {
            string query = "INSERT INTO `" + table + "`(" + columns + ") VALUES(" + values + ")";

            return CompleteQuery(query);
        }
    }
}
