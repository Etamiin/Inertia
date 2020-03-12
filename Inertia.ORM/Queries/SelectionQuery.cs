using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.ORM
{
    public class SelectionQuery : IDisposable
    {
        #region Private variables

        private Table Table;
        private MySqlCommand Command;
        private ConditionQuery ConditionQuery;
        private List<string> Fields;

        #endregion

        #region Constructors

        internal SelectionQuery(Table table) : this(table, -1)
        {
        }
        internal SelectionQuery(Table table, int limit)
        {
            try
            {
                Command = table.Database.CreateCommand();
                Table = table;
                ConditionQuery = new ConditionQuery(Command, limit);
                Fields = new List<string>();
            }
            catch (Exception e)
            {
                InertiaLoggerORM.Error(e);
            }
        }
        internal SelectionQuery(Table table, ConditionQuery conditionQuery, int limit) : this(table, limit)
        {
            try
            {
                Command = conditionQuery.Command;
                Table = table;
                ConditionQuery = conditionQuery;
                Fields = new List<string>();
            }
            catch (Exception e)
            {
                InertiaLoggerORM.Error(e);
            }
        }

        #endregion

        public SelectionQuery AddCondition(string field, object value, ConditionType type = ConditionType.AND)
        {
            ConditionQuery.AddCondition(field, value, type);
            return this;
        }

        public SelectionQuery Add(string field)
        {
            Fields.Add(field);
            return this;
        }
        public SelectionQuery AddRange(params string[] fields)
        {
            Fields.AddRange(fields);
            return this;
        }

        public SelectionResult Select()
        {
            var query = "SELECT ";

            if (Fields.Count > 0) {
                for (var i = 0; i < Fields.Count; i++)
                {
                    query += Fields[i];
                    if (i < Fields.Count - 1)
                        query += ",";
                }
            }
            else
                query += "*";

            query += " FROM `" + Table.TableName + "`";

            ConditionQuery.JoinQuery(ref query);

            try
            {
                Command.CommandText = query;
                var reader = Command.ExecuteReader();

                return new SelectionResult(reader);
            }
            catch (Exception ex)
            {
                InertiaLoggerORM.Error(ex);
                return null;
            }
        }

        public void Dispose()
        {
            ConditionQuery.Dispose();
            Command.Dispose();
            Fields.Clear();
            Fields = null;
            ConditionQuery = null;
            Command = null;
            Table = null;
        }
    }
}
