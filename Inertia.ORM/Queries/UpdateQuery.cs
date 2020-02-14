using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.ORM
{
    public class UpdateQuery : IDisposable
    {
        #region Private variables

        private Table Table;
        private MySqlCommand Command;
        private ConditionQuery ConditionQuery;
        private List<string> Values;

        #endregion

        #region Constructors

        internal UpdateQuery(Table table) : this(table, -1)
        {
        }
        internal UpdateQuery(Table table, int limit)
        {
            try
            {
                Command = table.Database.CreateCommand();
                Table = table;
                ConditionQuery = new ConditionQuery(Command, limit);
                Values = new List<string>();
            }
            catch (Exception e)
            {
                LoggerORM.Error(e);
            }
        }
        internal UpdateQuery(Table table, ConditionQuery conditionQuery, int limit) : this (table, limit)
        {
            try
            {
                Command = conditionQuery.Command;
                Table = table;
                ConditionQuery = conditionQuery;
                Values = new List<string>();
            }
            catch (Exception e)
            {
                LoggerORM.Error(e);
            }
        }

        #endregion

        public UpdateQuery AddCondition(string field, object value, ConditionType type = ConditionType.AND)
        {
            ConditionQuery.AddCondition(field, value, type);
            return this;
        }

        public UpdateQuery Add(string field, object value)
        {
            Command?.Parameters.AddWithValue("@" + field, value);
            Values.Add((Values.Count > 0 ? "," : string.Empty) + field + "=@" + field);

            return this;
        }
        public UpdateQuery AddRange(string[] fields, object[] values)
        {
            if (fields.Length != values.Length)
                throw new IndexOutOfRangeException();

            for (var i = 0; i < fields.Length; i++)
                Add(fields[i], values[i]);
            return this;
        }

        public int Update()
        {
            if (Values.Count == 0)
                throw new ArgumentNullException();


            var query = "UPDATE `" + Table.Name + "` SET ";
            foreach (var setter in Values)
                query += setter;

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
            Values.Clear();
            ConditionQuery = null;
            Command = null;
            Values = null;
            Table = null;
        }
    }
}
