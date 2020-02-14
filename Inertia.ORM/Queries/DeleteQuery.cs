using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.ORM
{
    public class DeleteQuery : IDisposable
    {
        #region Private variables

        private Table Table;
        private MySqlCommand Command;
        private ConditionQuery ConditionQuery;

        #endregion

        #region Constructors

        internal DeleteQuery(Table table) : this(table, -1)
        {
        }
        internal DeleteQuery(Table table, int limit)
        {
            try
            {
                Command = table.Database.CreateCommand();
                Table = table;
                ConditionQuery = new ConditionQuery(Command, limit);
            }
            catch (Exception e)
            {
                LoggerORM.Error(e);
            }
        }
        internal DeleteQuery(Table table, ConditionQuery conditionQuery, int limit) : this(table, limit)
        {
            try
            {
                Command = conditionQuery.Command;
                Table = table;
                ConditionQuery = conditionQuery;
            }
            catch (Exception e)
            {
                LoggerORM.Error(e);
            }
        }

        #endregion

        public DeleteQuery AddCondition(string field, object value, ConditionType type = ConditionType.AND)
        {
            ConditionQuery.AddCondition(field, value, type);
            return this;
        }

        public int Delete()
        {
            var query = "DELETE FROM `" + Table.Name + "`";
            ConditionQuery.JoinQuery(ref query);

            try
            {
                Command.CommandText = query;
                return Command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                LoggerORM.Error(ex);
                return 0;
            }
        }
        public void Dispose()
        {
            ConditionQuery.Dispose();
            Command.Dispose();
            Command = null;
            ConditionQuery = null;
            Table = null;
        }
    }
}
