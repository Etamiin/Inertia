using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Inertia.ORM
{
    /// <summary>
    ///
    /// </summary>
    public static class SqlManager
    {
        private static Dictionary<string, Database> _databases;

        private static bool _initialized;
        private static AutoQueueExecutor _queue;

        internal static void EnqueueAsyncOperation(BasicAction action)
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
            Initialize();

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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="databaseType"></param>
        /// <param name="database"></param>
        /// <returns></returns>
        public static bool TrySearchDatabase(Type databaseType, out Database database)
        {
            Initialize();

            database = _databases.Values.FirstOrDefault((db) => db.GetType() == databaseType);
            return database != null;
        }

        /// <summary>
        /// Try to find the specified <typeparamref name="T"/> <see cref="Database"/> and execute the specified action with it
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="onDatabase"></param>
        public static void TryUseDatabase<T>(BasicAction<T> onDatabase) where T : Database
        {
            if (TrySearchDatabase(out T db))
            {
                onDatabase?.Invoke(db);
            }
        }

        private static void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;

            if (_databases == null)
            {
                _databases = new Dictionary<string, Database>();
            }
            if (_queue == null)
            {
                _queue = new AutoQueueExecutor();
            }

            var namedDbDict = new Dictionary<string, List<Type>>();
            var typedDbDict = new Dictionary<Type, List<Type>>();

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
                            var attachTo = type.GetCustomAttribute<AttachTo>(false);
                            if (attachTo != null)
                            {
                                if (!string.IsNullOrEmpty(attachTo.DatabaseName))
                                {
                                    if (namedDbDict.TryGetValue(attachTo.DatabaseName, out List<Type> types))
                                    {
                                        types.Add(type);
                                    }
                                    else
                                    {
                                        namedDbDict.Add(attachTo.DatabaseName, new List<Type> { type });
                                    }
                                }
                                else if (attachTo.DatabaseType != null)
                                {
                                    if (typedDbDict.TryGetValue(attachTo.DatabaseType, out List<Type> types))
                                    {
                                        types.Add(type);
                                    }
                                    else
                                    {
                                        typedDbDict.Add(attachTo.DatabaseType, new List<Type> { type });
                                    }
                                }
                            }
                        }
                    }
                }

                foreach (var pair in namedDbDict)
                {
                    TryRegisterDb(pair.Value, dbName: pair.Key);
                }
                foreach (var pair in typedDbDict)
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

                    if (db != null && db.GetType().GetCustomAttribute<AutoGenerateTables>() != null)
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

                    db.OnInitialized();
                }
            }
            catch (Exception ex)
            {
                _databases.Clear();
                throw new InitializationFailedException(ex.ToString());
            }
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