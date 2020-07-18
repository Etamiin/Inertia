using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Inertia.Internal;

namespace Inertia.ORM
{
    /// <summary>
    /// Represent the database class
    /// </summary>
    public abstract class Database
    {
        #region Static methods

        private static Dictionary<string, Database> m_databases;

        /// <summary>
        /// Create all tables for each <see cref="Database"/> class
        /// </summary>
        public static void CreateAllTables()
        {
            if (m_databases == null)
                LoadDatabases();

            foreach (var db in m_databases)
                db.Value.CreateTables();
        }

        /// <summary>
        /// Get a <see cref="Database"/> class instance based on the name
        /// </summary>
        /// <param name="name">Name of the <see cref="Database"/></param>
        /// <returns>Finded instance or null</returns>
        public static Database GetDatabase(string name)
        {
            if (m_databases == null)
                LoadDatabases();

            m_databases.TryGetValue(name, out Database database);
            return database;
        }
        /// <summary>
        /// Get a <see cref="Database"/> class instance based on the <see cref="Type"/>
        /// </summary>
        /// <typeparam name="T"><see cref="Type"/> of the target <see cref="Database"/></typeparam>
        /// <returns><typeparamref name="T"/> instance or null</returns>
        public static T GetDatabase<T>() where T : Database
        {
            var database = GetDatabase(typeof(T));
            if (database != null)
                return (T)database;

            return null;
        }
        /// <summary>
        /// Get a <see cref="Database"/> class instance based on the <see cref="Type"/>
        /// </summary>
        /// <typeparam name="T"><see cref="Type"/> of the target <see cref="Database"/></typeparam>
        /// <returns><typeparamref name="T"/> instance or null</returns>
        public static Database GetDatabase(Type databaseType)
        {
            if (m_databases == null)
                LoadDatabases();

            foreach (var db in m_databases.Values)
            {
                if (db.GetType() == databaseType)
                    return db;
            }

            return null;
        }

        /// <summary>
        /// Get the specified <typeparamref name="T"/> instance and execute <paramref name="usage"/> action
        /// </summary>
        /// <typeparam name="T"><see cref="Type"/> of <see cref="Database"/> to get</typeparam>
        /// <param name="usage">Action to execute</param>
        public static void Use<T>(BasicAction<T> usage) where T : Database
        {
            var db = GetDatabase<T>();
            if (db != null)
                usage(db);
        }

        private static void LoadDatabases()
        {
            m_databases = new Dictionary<string, Database>();

            var Assemblys = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in Assemblys)
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (type.IsAbstract)
                        continue;

                    if (type.IsClass && type.IsSubclassOf(typeof(Database)))
                    {
                        var autoCreateAttr = type.GetCustomAttribute<AutoGenerateTables>();
                        var database = (Database)OrmHelper.InstantiateObject(type);
                        if (database != null && !m_databases.ContainsKey(database.Name))
                        {
                            database.AutoCreateTables = autoCreateAttr != null;
                            m_databases.Add(database.Name, database);
                        }
                    }
                }
            }

            LoadTables();

            foreach (var db in m_databases)
            {
                if (db.Value.AutoCreateTables)
                    db.Value.CreateTables();
            }
        }
        private static void LoadTables()
        {
            var Assemblys = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in Assemblys)
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (type.IsAbstract)
                        continue;

                    if (type.IsClass && type.IsSubclassOf(typeof(Table)))
                    {
                        var table = (Table)OrmHelper.InstantiateObject(type);
                        if (table != null)
                        {
                            table.LoadInstance();
                            if (table.Database != null && !table.Database.Tables.Contains(table))
                                table.Database.Tables.Add(table);
                        }
                    }
                }
            }
        }

        #endregion

        #region Public variables

        /// <summary>
        /// Get the name of the database
        /// </summary>
        public abstract string Name { get; }
        /// <summary>
        /// Get the host ip of the database
        /// </summary>
        public abstract string Host { get; }
        /// <summary>
        /// Get the username to use for the connection
        /// </summary>
        public abstract string User { get; }
        /// <summary>
        /// Get the password to use for the connection
        /// </summary>
        public abstract string Password { get; }
        /// <summary>
        /// Get the port to use for the connection
        /// </summary>
        public virtual int Port { get => 3306; }

        #endregion

        #region Internal variables

        internal bool AutoCreateTables;
        internal List<Table> Tables;

        #endregion

        #region Constructors

        /// <summary>
        /// Instantiate a new instance of the class <see cref="Database"/>
        /// </summary>
        public Database()
        {
            Tables = new List<Table>();
        }

        #endregion

        /// <summary>
        /// Create all tables of the current <see cref="Database"/>
        /// </summary>
        public void CreateTables()
        {
            foreach (var table in Tables)
                CreateTable(table);
        }
        /// <summary>
        /// Create specified <typeparamref name="T"/> table of the current <see cref="Database"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void CreateTable<T>() where T : Table
        {
            foreach (var table in Tables)
            {
                if (table.GetType() == typeof(T))
                {
                    CreateTable(table);
                    break;
                }
            }
        }
        /// <summary>
        /// Delete specified <typeparamref name="T"/> table of the current <see cref="Database"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void DropTable<T>() where T : Table
        {
            foreach (var table in Tables)
            {
                if (table.GetType() == typeof(T))
                {
                    DropTable(table);
                    break;
                }
            }
        }

        /// <summary>
        /// Delete all rows in the specified <typeparamref name="T"/> table
        /// </summary>
        /// <param name="onDeleted">Callback to call when delete query is executed</param>
        /// <typeparam name="T"><see cref="Type"/> of the table</typeparam>
        public void DeleteAll<T>(BasicAction onDeleted = null) where T : Table
        {
            SqlManager.AsyncOperation(this, (db) => {
                ExecuteQueryFromInstance<T>(QueryType.DeleteAll, null, null, null, "Executing truncate query failed: {0}");
                return null;
            }, (success, obj) => {
                onDeleted?.Invoke();
            });
        }
        /// <summary>
        /// Delete all rows based on the specified <see cref="ConditionBuilder"/> in specified <typeparamref name="T"/> table
        /// </summary>
        /// <param name="condition"><see cref="ConditionBuilder"/> condition of the deletion</param>
        /// <param name="onDeleted">Callback to call when delete query is executed</param>
        /// <typeparam name="T"><see cref="Type"/> of the table</typeparam>
        public void Delete<T>(ConditionBuilder condition, BasicAction onDeleted = null) where T : Table
        {
            if (condition == null)
                throw new NullReferenceException();

            SqlManager.AsyncOperation(this, (db) => {
                ExecuteQueryFromInstance<T>(QueryType.Delete, condition, null, null, "Executing delete query failed: {0}");
                return null;
            }, (success, obj) => {
                onDeleted?.Invoke();
            });
        }
        /// <summary>
        /// Select all values based on the specified <see cref="ConditionBuilder"/> in the specified <typeparamref name="T"/> table
        /// </summary>
        /// <param name="condition"><see cref="ConditionBuilder"/> for the selection</param>
        /// <param name="onSelected">Callback to call when select query is executed</param>
        /// <param name="columns">Columns to select</param>
        /// <typeparam name="T"><see cref="Type"/> of the table</typeparam>
        /// <returns><typeparamref name="T"/> array</returns>
        public void Select<T>(ConditionBuilder condition, BasicAction<T[]> onSelected = null, params string[] columns) where T : Table
        {
            SqlManager.AsyncOperation(this, (db) => {
                var result = new List<T>();

                ExecuteQueryFromInstance<T>(QueryType.Select, condition, columns, (reader) => {
                    while (reader.Read())
                    {
                        var instance = OrmHelper.InstantiateObject(typeof(T)) as T;

                        for (var i = 0; i < reader.FieldCount; i++)
                        {
                            var field = instance.GetType().GetField(reader.GetName(i));
                            if (field != null)
                            {
                                var value = reader.GetValue(i);
                                if (value.GetType() == typeof(DBNull))
                                    value = null;

                                field.SetValue(instance, value);
                            }
                        }

                        result.Add(instance);
                    }
                }, "Executing select query failed: {0}");

                return result.ToArray();
            }, (success, obj) => {
                onSelected?.Invoke((T[])obj);
            });
        }
        /// <summary>
        /// Select specified columns of each rows in the specified <typeparamref name="T"/> table
        /// </summary>
        /// <param name="columns">Columns to select</param>
        /// <param name="onSelected">Callback to call when select query is executed</param>
        /// <typeparam name="T"><see cref="Type"/> of the table</typeparam>
        /// <returns><typeparamref name="T"/> array</returns>
        public void SelectAll<T>(BasicAction<T[]> onSelected = null, params string[] columns) where T : Table
        {
            Select(null, onSelected, columns);
        }
        /// <summary>
        /// Get the number of rows based on the specified <see cref="ConditionBuilder"/> in the specified <typeparamref name="T"/> table
        /// </summary>
        /// <param name="condition"><see cref="ConditionBuilder" /> to use</param>
        /// <param name="onCounted">Callback to call when count query is executed</param>
        /// <param name="columnName">Column name to focus on (or all in not specified)</param>
        /// <typeparam name="T"><see cref="Type"/> of the table</typeparam>
        /// <returns>The number of rows</returns>
        public void Count<T>(ConditionBuilder condition, BasicAction<long> onCounted,  string columnName = "*") where T : Table
        {
            SqlManager.AsyncOperation(this, (db) => {
                var count = 0L;

                ExecuteQueryFromInstance<T>(QueryType.Count, condition, columnName, (reader) => {
                    if (reader.Read())
                        count = (long)reader.GetValue(0);
                }, "Executing count query failed: {0}");

                return count;
            }, (success, obj) => {
                onCounted((long)obj);
            });
        }
        /// <summary>
        /// Return true if a value based on the specified <see cref="ConditionBuilder"/> in the <typeparamref name="T"/> table exist
        /// </summary>
        /// <param name="condition"><see cref="ConditionBuilder"/> to use</param>
        /// <param name="onChecked">Callback to call when query is executed</param>
        /// <typeparam name="T"><see cref="Type"/> of the table</typeparam>
        /// <returns>True if exist</returns>
        public void Exist<T>(ConditionBuilder condition, BasicAction<bool> onChecked) where T : Table
        {
            condition.SetLimit(1);
            Count<T>(condition, (count) => onChecked(count > 0));
        }
        /// <summary>
        /// Update rows based on the specified <see cref="ConditionBuilder"/> by the specified <typeparamref name="T"/> values
        /// </summary>
        /// <param name="condition">Conditions for update</param>
        /// <param name="onUpdated">Callback to call when query update is executed</param>
        /// <param name="reference">Reference instance</param>
        public void Update<T>(ConditionBuilder condition, T reference, BasicAction onUpdated = null) where T : Table
        {
            SqlManager.AsyncOperation(this, (db) => {
                reference.ExecuteQuery(QueryType.Update, condition, null, null, "Executing update query failed: {0}");
                return null;
            }, (success, obj) => {
                onUpdated?.Invoke();
            });
        }

        internal MySqlConnection CreateConnection()
        {
            var conn = new MySqlConnection("server=" + Host + ";uid=" + User + ";pwd=" + Password + ";database=" + Name + ";port=" + Port);
            conn.Open();

            return conn;
        }
        internal MySqlCommand CreateCommand()
        {
            using (var command = new MySqlCommand()
            {
                Connection = CreateConnection()
            })
            {
                if (command.Connection == null)
                    return null;

                command.Prepare();
                return command;
            }
        }
        internal MySqlCommand CreateCommand(string query)
        {
            var command = CreateCommand();
            command.CommandText = query;

            return command;
        }

        private T GetTable<T>() where T : Table
        {
            foreach (var table in Tables)
                if (table.GetType() == typeof(T))
                    return (T)table;

            return null;
        }

        private void CreateTable(Table table)
        {
            if (table.IgnoreCreation)
                return;

            SqlManager.AsyncOperation(this, (db) => {
                return table.ExecuteQuery(QueryType.CreateTable, null, null, null, "Executing create table query failed: {0}");
            });
        }
        private void DropTable(Table table)
        {
            SqlManager.AsyncOperation(this, (db) => {
                return table.ExecuteQuery(QueryType.DropTable, null, null, null, "Executing drop table query failed: {0}");
            });
        }
        private void ExecuteQueryFromInstance<T>(QueryType queryType, ConditionBuilder condition, object data, BasicAction<MySqlDataReader> onReader, string errorMessage) where T : Table
        {
            var table = GetTable<T>();
            if (table == null)
                throw new InvalidDatabaseAttachException(GetType());

            table.ExecuteQuery(queryType, condition, data, onReader, errorMessage);
        }
    }
}
