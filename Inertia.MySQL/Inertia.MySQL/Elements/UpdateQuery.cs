using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.MySQL
{
    public class UpdateQuery : BasicQuery
    {
        private string setterQuery;

        public UpdateQuery(string table) : base(table)
        {
        }

        public new UpdateQuery AddCondition(string column, object value)
        {
            base.AddCondition(column, value);
            return this;
        }
        public new UpdateQuery AddLmit(int limit)
        {
            base.AddLmit(limit);
            return this;
        }

        public UpdateQuery AddValue(string column, object value)
        {
            command.Parameters.AddWithValue("@" + column, value);
            if (!string.IsNullOrEmpty(setterQuery))
                setterQuery += ",";
            setterQuery += column + "=@" + column;

            return this;
        }

        public bool Execute()
        {
            if (command == null || string.IsNullOrEmpty(setterQuery))
                return false;

            try {
                command.CommandText = GenerateQuery();
                return command.ExecuteNonQuery() != 0;
            }
            catch (Exception e) {
                InDebug.Error(e);
                return false;
            }
        }

        public override string GenerateQuery()
        {
            string query = "UPDATE `" + table + "` SET " + setterQuery;
            return CompleteQuery(query);
        }
    }
}
