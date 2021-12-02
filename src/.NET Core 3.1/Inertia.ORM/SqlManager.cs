using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Inertia.ORM
{
    public static class SqlManager
    {
        private static Dictionary<string, Database> _databases;

        private static AutoQueueExecutor _queue;

        static SqlManager()
        {
            _databases = new Dictionary<string, Database>();
            _queue = new AutoQueueExecutor();

            var namedDb = new Dictionary<string, List<Type>>();
            var typedDb = new Dictionary<Type, List<Type>>();

            try
            {
                var assemblys = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assemblys)
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (type.IsAbstract || !type.IsClass)
                        {
                            continue;
                        }

                        if (type.IsSubclassOf(typeof(Database)))
                        {
                            var db = (Database)Activator.CreateInstance(type);
                            if (!_databases.ContainsKey(db.Name))
                            {
                                _databases.Add(db.Name, db);
                                db.TryCreateItSelf();
                            }
                            else
                            {
                                throw new DatabaseAlreadyInitializedException(db.Name);
                            }
                        }
                        else if (type.IsSubclassOf(typeof(Table)))
                        {
                            var link = type.GetCustomAttribute<TableLink>(false);
                            if (link != null)
                            {
                                if (!string.IsNullOrEmpty(link.DatabaseName))
                                {
                                    if (namedDb.TryGetValue(link.DatabaseName, out List<Type> types))
                                    {
                                        types.Add(type);
                                    }
                                    else
                                    {
                                        namedDb.Add(link.DatabaseName, new List<Type> { type });
                                    }
                                }
                                else if (link.DatabaseType != null)
                                {
                                    if (typedDb.TryGetValue(link.DatabaseType, out List<Type> types))
                                    {
                                        types.Add(type);
                                    }
                                    else
                                    {
                                        typedDb.Add(link.DatabaseType, new List<Type> { type });
                                    }
                                }
                            }
                        }
                    }
                }

                foreach (var pair in namedDb)
                {
                    TryRegisterDb(pair.Value, dbName: pair.Key);
                }
                foreach (var pair in typedDb)
                {
                    TryRegisterDb(pair.Value, dbType: pair.Key);
                }

                void TryRegisterDb(List<Type> tables, string dbName = "", Type dbType = null)
                {
                    Database db = null;
                    if (!string.IsNullOrEmpty(dbName))
                    {
                        TrySearchDatabase(dbName, out db);
                    }
                    else if (dbType != null)
                    {
                        TrySearchDatabase(dbType, out db);
                    }

                    if (db != null && db.AutoGenerateTable)
                    {
                        RegisterTablesTo(db, tables);
                    }
                }
                void RegisterTablesTo(Database db, List<Type> types)
                {
                    foreach (var type in types)
                    {
                        var table = (Table)Activator.CreateInstance(type);
                        db.Create(table);
                    }

                    db.IsInitialized = true;
                    db.OnInitialized();
                }
            }
            catch (Exception ex)
            {
                _databases.Clear();
                throw new InitializationFailedException(ex.ToString());
            }
        }

        internal static void PoolAsyncOperation(BasicAction action)
        {
            _queue.Enqueue(action);
        }

        /// <summary>
        /// Returns a <see cref="Database"/> already registered.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="database"></param>
        /// <returns></returns>
        public static bool TrySearchDatabase(string name, out Database database)
        {
            return _databases.TryGetValue(name, out database);
        }
        /// <summary>
        /// Returns a <typeparamref name="T"/> already registered.
        /// </summary>
        /// <returns></returns>
        public static bool TrySearchDatabase<T>(out T database) where T : Database
        {
            if (TrySearchDatabase(typeof(T), out Database db))
            {
                database = (T)db;
                return true;
            }

            database = default;
            return false;
        }
        public static bool TrySearchDatabase(Type databaseType, out Database database)
        {
            database = _databases.Values.FirstOrDefault((db) => db.GetType() == databaseType);
            return database != null;
        }

        internal static bool CreateTableInstance<T>(out T instance) where T : Table
        {
            instance = null;
            try
            {
                instance = Activator.CreateInstance<T>();
                return true;
            }
            catch { return false; }
        }
        internal static void SetQuery(this MySqlCommand command, string query, SqlCondition condition = null)
        {
            command.CommandText = query;
            if (condition != null)
            {
                condition.ApplyToCmd(command);
            }

            command.Prepare();
        }
        internal static void OnReader(this MySqlCommand command, BasicAction<MySqlDataReader> readerCallback)
        {
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    readerCallback(reader);
                }
            }
        }
    }
}