using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.MySQL
{
    public class DeleteQuery : BasicQuery
    {
        private bool confirmed;

        public DeleteQuery(string table) : base(table)
        {
        }

        public new DeleteQuery AddCondition(string column, object value)
        {
            base.AddCondition(column, value);
            return this;
        }
        public new DeleteQuery AddLmit(int limit)
        {
            base.AddLmit(limit);
            return this;
        }

        public DeleteQuery CompleteDeletionSecure()
        {
            confirmed = true;
            return this;
        }

        public bool Execute()
        {
            if (command == null || !HasCondition && !confirmed)
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
            string query = "DELETE FROM `" + table + "`";
            return CompleteQuery(query);
        }
    }
}
