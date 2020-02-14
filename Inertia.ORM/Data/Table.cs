using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.ORM
{
    public abstract class Table
    {
        #region Static methods

        internal static void LoadTables()
        {
            var Assemblys = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in Assemblys)
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (type.IsClass && type.IsSubclassOf(typeof(Table)))
                    {
                        var table = (Table)type.GetConstructor(Type.EmptyTypes).Invoke(new object[] { });
                        if (table != null)
                        {
                            var database = Database.GetDatabase(table.DatabaseName);
                            if (database == null)
                                continue;

                            table.Database = database;
                            database.AddTable(table);
                        }
                    }
                }
            }
        }
        internal static void CreateTables(Database[] databases)
        {
            foreach (var database in databases)
            {
                var tables = database.GetTables();
                foreach (var table in tables)
                {
                    var fields = table.GetType().GetFields();
                    table.Create(fields);
                }
            }
        }

        public static Table GetTable(string databaseName, string tableName)
        {
            return Database.GetDatabase(databaseName).GetTable(tableName);
        }

        #endregion

        #region Public variables

        public Database Database { get; internal set; }
        public abstract string Name { get; }
        public abstract string DatabaseName { get; }

        #endregion

        internal void Create(FieldInfo[] fields)
        {
            if (Database == null)
                return;

            try
            {
                var command = Database.CreateCommand();

                var query = "CREATE TABLE IF NOT EXISTS `" + Name + "` (";
                var fieldsContent = string.Empty;
                var primaryColumns = "primary key(";

                for (var i = 0; i < fields.Length; i++)
                {
                    var field = fields[i];
                    var attributes = field.GetCustomAttributes(false);
                    var currentField = "`" + field.Name + "` ";
                    var hasSqlAttribute = false;

                    foreach (var attribute in attributes)
                    {
                        if (attribute is VarcharField varchar)
                        {
                            currentField += "VARCHAR(" + varchar.Length + ")";
                            hasSqlAttribute = true;
                        }
                    }

                    if (!hasSqlAttribute)
                    {
                        var fieldType = FieldType.GetFieldType(field.FieldType);
                        currentField += fieldType.SqlType.ToUpper() + (fieldType.Unsigned ? " UNSIGNED" : string.Empty);
                    }

                    foreach (var attribute in attributes)
                    {
                        if (attribute is PrimaryKeyField primaryKey)
                        {
                            primaryColumns += "`" + field.Name + "`,";
                            if (primaryKey.IsAutoIncrement)
                                currentField += " AUTO_INCREMENT";
                        }
                    }

                    if (i < fields.Length - 1)
                        currentField += ",";

                    fieldsContent += currentField;
                }

                if (primaryColumns.EndsWith(","))
                    primaryColumns = primaryColumns.Substring(0, primaryColumns.Length - 1);
                if (!string.IsNullOrEmpty(fieldsContent))
                {
                    if (!fieldsContent.EndsWith(",") && !string.IsNullOrEmpty(primaryColumns))
                        fieldsContent += ",";

                    query += fieldsContent + primaryColumns + ")";
                }

                command.CommandText = query + ")";
                command.ExecuteNonQuery();

                command.Dispose();
            }
            catch (Exception e)
            {
                LoggerORM.Error("Creating table {0} on database {1} failed: {2}", Name, Database.Name, e);
            }
        }

        public ConditionQuery CreateCondition()
        {
            return CreateCondition(-1);
        }
        public ConditionQuery CreateCondition(int limit)
        {
            return new ConditionQuery(Database.CreateCommand(), limit);
        }
        public InsertionQuery CreateInsertion()
        {
            return new InsertionQuery(this);
        }
        public SelectionQuery CreateSelection()
        {
            return new SelectionQuery(this);
        }
        public SelectionQuery CreateSelection(int limit)
        {
            return new SelectionQuery(this, limit);
        }
        public UpdateQuery CreateUpdate()
        {
            return new UpdateQuery(this);
        }
        public UpdateQuery CreateUpdate(int limit)
        {
            return new UpdateQuery(this, limit);
        }
        public DeleteQuery CreateDeletion()
        {
            return new DeleteQuery(this);
        }
        public DeleteQuery CreateDeletion(int limit)
        {
            return new DeleteQuery(this, limit);
        }

        public long Insert(string[] fields, object[] values)
        {
            var query = CreateInsertion();
            var id = query
                .AddRange(fields, values)
                .Insert()
                .LastInsertedId;

            query.Dispose();
            return id;
        }
        public SelectionResult Select(ConditionQuery conditionQuery, params string[] fields)
        {
            return Select(conditionQuery, -1, fields);
        }
        public SelectionResult Select(ConditionQuery conditionQuery, int limit, params string[] fields)
        {
            var query = new SelectionQuery(this, conditionQuery, limit).AddRange(fields);
            var result = query.Select();

            query.Dispose();
            return result;
        }
        public int Update(ConditionQuery conditionQuery, string[] fields, object[] values)
        {
            return Update(conditionQuery, -1, fields, values);
        }
        public int Update(ConditionQuery conditionQuery, int limit, string[] fields, object[] values)
        {
            var query = new UpdateQuery(this, conditionQuery, limit).AddRange(fields, values);
            var countUpdated = query.Update();

            query.Dispose();
            return countUpdated;
        }
        public int Delete(ConditionQuery conditionQuery)
        {
            return Delete(conditionQuery, -1);
        }
        public int Delete(ConditionQuery conditionQuery, int limit)
        {
            var query = new DeleteQuery(this, conditionQuery, limit);
            var countDeleted = query.Delete();

            query.Dispose();
            return countDeleted;
        }

        public long GetCount()
        {
            return GetCount(CreateCondition());
        }
        public long GetCount(ConditionQuery conditionQuery)
        {
            var command = conditionQuery.Command;
            var query = "SELECT COUNT(*) FROM `" + Name + "`";

            conditionQuery.JoinQuery(ref query);

            try
            {
                command.CommandText = query;
                var reader = command.ExecuteReader();

                var count = reader.Read() ? reader.GetInt64(0) : 0;
                reader.Dispose();

                return count;
            }
            catch (Exception e)
            {
                LoggerORM.Error(e);
                return 0;
            }
        }

        public bool Exist(ConditionQuery conditionQuery)
        {
            return GetCount(conditionQuery) > 0;
        }
    }
}
