using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.ORM
{
    public class InsertionQuery : IDisposable
    {
        #region Public variables

        public long LastInsertedId { get; private set; }

        #endregion

        #region Private variables

        private Table Table;
        private MySqlCommand Command;
        private Dictionary<string, object> Values;

        #endregion

        #region Constructors

        internal InsertionQuery(Table table)
        {
            try
            {
                Command = table.Database.CreateCommand();
                Table = table;
                Values = new Dictionary<string, object>();
            }
            catch (Exception e)
            {
                LoggerORM.Error(e);
            }
        }
        
        #endregion

        public object this[string field]
        {
            set
            {
                if (Values.ContainsKey(field))
                {
                    Command.Parameters.RemoveAt("@" + field);
                    Values[field] = value;
                }
                else
                    Values.Add(field, value);

                Command.Parameters.AddWithValue("@" + field, value);
            }
        }
        public InsertionQuery AddRange(string[] fields, object[] values)
        {
            if (fields.Length != values.Length)
                throw new ArgumentOutOfRangeException();

            for (var i = 0; i < fields.Length; i++)
                this[fields[i]] = values[i];

            return this;
        }

        public InsertionQuery Insert()
        {
            if (Values.Count == 0)
                return this;

            var query = "INSERT INTO `" + Table.Name + "`";
            var fields = "(";
            var values = "(";
            var keys = Values.Keys.ToArray();

            for (var i = 0; i < keys.Length; i++)
            {
                fields += keys[i];
                values += "@" + keys[i];
                if (i < keys.Length - 1)
                {
                    fields += ",";
                    values += ",";
                }
            }

            fields += ")";
            values += ")";

            query += fields + " VALUES " + values;

            try
            {
                Command.CommandText = query;
                Command.ExecuteNonQuery();

                LastInsertedId = Command.LastInsertedId;
            }
            catch (Exception ex)
            {
                LoggerORM.Error(ex);
            }

            return this;
        }

        public void Dispose()
        {
            Command.Dispose();
            Values.Clear();
            Command = null;
            Table = null;
            Values = null;
        }
    }
}
