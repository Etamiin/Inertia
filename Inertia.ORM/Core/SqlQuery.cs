using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using MySql.Data.MySqlClient;
using Inertia;
using Inertia.Internal;

namespace Inertia.ORM
{
    public class SqlQuery<T> where T : Table
    {
        internal static SqlQuery<T> CreateTable(T table)
        {
            return new SqlQuery<T>(table, QueryType.CreateTable); ;
        }
        internal static SqlQuery<T> DeleteTable(T table)
        {
            return new SqlQuery<T>(table, QueryType.DeleteTable);
        }
        internal static SqlQuery<T> Insert(T table)
        {
            return new SqlQuery<T>(table, QueryType.Insert);
        }
        
        public static SqlQuery<T> BeginUpdate(T table, params string[] columnsToUpdate)
        {
            var query = new SqlQuery<T>(table, QueryType.Update)
            {
                m_selectedColumns = columnsToUpdate
            };

            return query;
        }
        public static SqlQuery<T> BeginSelect(params string[] columnsToSelect)
        {
            var table = SqlManager.GetTable<T>();
            if (table == null)
                return null;

            var query = new SqlQuery<T>(table, QueryType.Select)
            {
                m_selectedColumns = columnsToSelect
            };

            return query;
        }
        public static SqlQuery<T> BeginDeleteRecords()
        {
            var table = SqlManager.GetTable<T>();
            if (table == null)
                return null;

            return new SqlQuery<T>(table, QueryType.Delete);
        }
        public static bool TruncateTable()
        {
            var table = SqlManager.GetTable<T>();
            if (table == null)
                return false;

            var query = new SqlQuery<T>(table, QueryType.Truncate);
            return query.Execute();
        }
        public static SqlQuery<T> BeginCount(string column = "*", bool distinct = false)
        {
            var table = SqlManager.GetTable<T>();
            if (table == null)
                return null;

            var query = new SqlQuery<T>(table, QueryType.Count)
            {
                m_selectedColumns = new string[] { column },
                m_countDistinct = distinct
            };

            return query;
        }
        public static SqlQuery<T> BeginExist()
        {
            var table = SqlManager.GetTable<T>();
            if (table == null)
                return null;

            var query = new SqlQuery<T>(table, QueryType.Exist);
            query.OnCondition().Limit(1);

            return query;
        }

        internal T Table;
        internal MySqlCommand Command;
        internal QueryType Type;
        
        private SqlCondition<T> m_condition;
        private string[] m_selectedColumns;
        private bool m_countDistinct;

        internal SqlQuery(T table, QueryType queryType)
        {
            Table = table;
            Command = Table.Database.CreateCommand();
            Type = queryType;
        }

        public SqlCondition<T> OnCondition()
        {
            if (m_condition == null)
                m_condition = new SqlCondition<T>(this);

            return m_condition;
        }
        public void AssignCondition(SqlCondition<T> condition)
        {
            if (condition == null)
                return;

            m_condition = condition;
        }
        public void UnassignCondition()
        {
            m_condition = null;
        }

        public bool Execute()
        {
            return SqlManager.ExecuteQuery(this, out _);
        }
        public void ExecuteAsync(BasicAction<bool, int> callback)
        {
            SqlManager.ExecuteQueryAsync(this, callback);
        }

        public T Select()
        {
            T instance = null;

            if (Type == QueryType.Select) {
                Command.CommandText = GetQuery();
                OnSqlReader((reader) => instance = Select(reader));
            }

            return instance;
        }
        public T[] SelectAll()
        {
            if (Type != QueryType.Select)
                return new T[] { };

            Command.CommandText = GetQuery();
            
            var records = new List<T>();
            OnSqlReader((reader) => records.Add(Select(reader)));

            return records.ToArray();
        }

        //Check for update -- 
        public long Count()
        {
            var count = 0L;

            if (Type == QueryType.Count || Type == QueryType.Exist) {
                Command.CommandText = GetQuery();
                OnSqlReader((reader) => count = (long)reader.GetValue(0));
            }

            return count;
        }
        public bool Exist()
        {
            return Count() > 0;
        }
        public long Average()
        {
            //TODO
            return 0;
        }
        public long Max()
        {
            //TODO
            return 0;
        }
        public long Min()
        {
            //TODO
            return 0;
        }
        public long Round()
        {
            //TODO (param int::nbr_decimal)
            return 0;
        }
        public long Sum()
        {
            //TODO
            return 0;
        }

        //--

        private string GetCreateTableQuery()
        {
            var builder = new StringBuilder("CREATE TABLE IF NOT EXISTS `" + Table.TableName + "` (");
            var primaryKeys = string.Empty;

            for (var i = 0; i < Table.Fields.Length; i++)
            {
                var field = Table.Fields[i];
                var fieldType = FieldType.GetFieldType(field.FieldType);
                var varChar = field.GetCustomAttribute<VarChar>();
                var isNotNull = field.GetCustomAttribute<NotNull>();
                var isPrimaryKey = field.GetCustomAttribute<PrimaryKey>();

                if (i > 0)
                    builder.Append(",");

                builder.Append("`" + field.Name + "` ");
                builder.Append(
                    fieldType.Code == TypeCode.String && varChar != null ?
                    "VARCHAR(" + varChar.Length + ")" :
                    fieldType.SqlType.ToUpper() + (fieldType.Unsigned ? " UNSIGNED" : string.Empty));

                if (isNotNull != null)
                {
                    builder.Append(" NOT NULL");
                    if (isNotNull.Unique)
                        builder.Append(" UNIQUE");
                }

                if (isPrimaryKey != null)
                {
                    if (!string.IsNullOrEmpty(primaryKeys))
                        primaryKeys += ",";

                    primaryKeys += "`" + field.Name + "`";
                    if (isPrimaryKey.AutoIncrement)
                        builder.Append(" AUTO_INCREMENT");
                }
            }

            if (!string.IsNullOrEmpty(primaryKeys))
                builder.Append(", primary key(" + primaryKeys + ")");

            builder.Append(")");

            return builder.ToString();
        }
        private string GetDeleteTableQuery()
        {
            return "DROP TABLE IF EXISTS " + Table.TableName;
        }
        private string GetInsertQuery()
        {
            var builder = new StringBuilder("INSERT INTO `" + Table.TableName + "`");
            var fields = string.Empty;

            for (var i = 0; i < Table.Fields.Length; i++)
            {
                if (i > 0)
                    fields += ",";

                var field = Table.Fields[i];
                fields += field.Name;

                Command.Parameters.AddWithValue("@" + field.Name, field.GetValue(Table));
            }

            builder.Append(" (" + fields + ") VALUES (" + fields.Replace(",", ",@") + ")");

            return builder.ToString();
        }
        private string GetUpdateQuery()
        {
            var builder = new StringBuilder("UPDATE `" + Table.TableName + "` SET ");

            IEnumerable<FieldInfo> fields = Table.Fields;

            if (m_selectedColumns.Length > 0)
            {
                var fieldList = new List<FieldInfo>();

                foreach (var column in m_selectedColumns)
                {
                    var field = Table.Fields.First((f) => f.Name == column);
                    if (field == null)
                        continue;

                    fieldList.Add(field);
                }

                fields = fieldList;
            }

            for (var i = 0; i < fields.Count(); i++)
            {
                var field = fields.ElementAt(i);

                var isPrimary = field.GetCustomAttribute<PrimaryKey>();
                if (fields.Count() > 1 && isPrimary != null)
                    continue;

                var paramName = SqlManager.GenerateRandomName();

                Command.Parameters.AddWithValue(paramName, field.GetValue(Table));
                builder.Append(field.Name + "=" + paramName);

                if (i < fields.Count() - 1)
                    builder.Append(", ");
            }

            return builder.ToString();
        }
        private string GetSelectQuery()
        {
            var builder = new StringBuilder("SELECT ");

            if (m_selectedColumns.Length > 0) {
                for (var i = 0; i < m_selectedColumns.Length; i++)
                {
                    var field = Table.Fields.First((f) => f.Name == m_selectedColumns[i]);
                    if (field == null)
                        continue;

                    builder.Append(m_selectedColumns[i]);
                    if (i < m_selectedColumns.Length - 1)
                        builder.Append(",");
                }
            }
            else
                builder.Append("*");

            builder.Append(" FROM `" + Table.TableName + "`");

            return builder.ToString();
        }
        private string GetDeleteQuery()
        {
            return "DELETE FROM `" + Table.TableName + "`";
        }
        private string GetTruncateQuery()
        {
            return "TRUNCATE TABLE `" + Table.TableName + "`";
        }
        private string GetCountQuery()
        {
            var builder = new StringBuilder("SELECT COUNT(");
            if (m_countDistinct)
                builder.Append("DISTINCT ");

            if (m_selectedColumns == null || m_selectedColumns.Length == 0 || string.IsNullOrEmpty(m_selectedColumns[0]))
                builder.Append("*");
            else
                builder.Append(m_selectedColumns[0]);

            builder.Append(") FROM `" + Table.TableName + "`");
            return builder.ToString();
        }

        private T Select(MySqlDataReader reader)
        {
            var instance = (Table)SqlManager.CreateInstance(typeof(T));
            for (var i = 0; i < reader.FieldCount; i++)
            {
                var field = instance.Fields.First((f) => f.Name == reader.GetName(i));
                if (field != null)
                {
                    var value = reader.GetValue(i);
                    if (value.GetType() == typeof(DBNull))
                        value = null;

                    if (field.FieldType == typeof(bool))
                        value = reader.GetBoolean(i);

                    field.SetValue(instance, value);
                }
            }

            return (T)instance;
        }

        internal void OnSqlReader(BasicAction<MySqlDataReader> onReader)
        {
            using (var reader = Command.ExecuteReader())
            {
                while (reader.Read())
                    onReader(reader);
            }
        }
        internal string GetQuery()
        {
            var query = string.Empty;

            switch (Type)
            {
                case QueryType.CreateTable:
                    return GetCreateTableQuery();
                case QueryType.DeleteTable:
                    return GetDeleteTableQuery();
                case QueryType.Insert:
                    return GetInsertQuery();
                case QueryType.Update:
                    query = GetUpdateQuery();
                    break;
                case QueryType.Select:
                    query = GetSelectQuery();
                    break;
                case QueryType.Delete:
                    query = GetDeleteQuery();
                    break;
                case QueryType.Truncate:
                    return GetTruncateQuery();
                case QueryType.Count:
                case QueryType.Exist:
                    query =  GetCountQuery();
                    break;
            }

            if (m_condition != null)
                query += " " + m_condition.GetQuery();

            return query;
        }
    }
}
