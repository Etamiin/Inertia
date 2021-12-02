using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Inertia.ORM
{
<<<<<<< HEAD
    /// <summary>
    ///
    /// </summary>
=======
>>>>>>> premaster
    public static class SqlManager
    {
        private static Dictionary<string, Database> _databases;

<<<<<<< HEAD
<<<<<<< HEAD
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

=======
=======
>>>>>>> premaster
        private static AutoQueueExecutor _queue;

        static SqlManager()
        {
<<<<<<< HEAD
>>>>>>> 9bfc85f6784b254a10c65f104446a83c8b195c40
            if (_databases == null)
            {
                _databases = new Dictionary<string, Database>();
            }
            if (_queue == null)
            {
                _queue = new AutoQueueExecutor();
            }

<<<<<<< HEAD
            var uTables = new Dictionary<string, List<Type>>();
            var utTables = new Dictionary<Type, List<Type>>();
=======
            var namedDbDict = new Dictionary<string, List<Type>>();
            var typedDbDict = new Dictionary<Type, List<Type>>();
>>>>>>> 9bfc85f6784b254a10c65f104446a83c8b195c40
=======
            _databases = new Dictionary<string, Database>();
            _queue = new AutoQueueExecutor();

            var namedDb = new Dictionary<string, List<Type>>();
            var typedDb = new Dictionary<Type, List<Type>>();
>>>>>>> premaster

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
<<<<<<< HEAD
<<<<<<< HEAD
                            if (_databases.ContainsKey(db.Name))
                            {
                                throw new DatabaseAlreadyInitializedException(db.Name);
                            }

                            db.TryCreateItSelf();
                            _databases.Add(db.Name, db);
=======
=======
>>>>>>> premaster
                            if (!_databases.ContainsKey(db.Name))
                            {
                                _databases.Add(db.Name, db);
                                db.TryCreateItSelf();
                            }
                            else
                            {
                                throw new DatabaseAlreadyInitializedException(db.Name);
                            }
<<<<<<< HEAD
>>>>>>> 9bfc85f6784b254a10c65f104446a83c8b195c40
                        }
                        else if (type.IsSubclassOf(typeof(Table)))
                        {
                            var attachTo = type.GetCustomAttribute<AttachTo>(false);
                            if (attachTo != null)
                            {
                                if (!string.IsNullOrEmpty(attachTo.DatabaseName))
                                {
<<<<<<< HEAD
                                    if (uTables.TryGetValue(attachTo.DatabaseName, out List<Type> types))
=======
                                    if (namedDbDict.TryGetValue(attachTo.DatabaseName, out List<Type> types))
>>>>>>> 9bfc85f6784b254a10c65f104446a83c8b195c40
=======
                        }
                        else if (type.IsSubclassOf(typeof(Table)))
                        {
                            var link = type.GetCustomAttribute<TableLink>(false);
                            if (link != null)
                            {
                                if (!string.IsNullOrEmpty(link.DatabaseName))
                                {
                                    if (namedDb.TryGetValue(link.DatabaseName, out List<Type> types))
>>>>>>> premaster
                                    {
                                        types.Add(type);
                                    }
                                    else
                                    {
<<<<<<< HEAD
<<<<<<< HEAD
                                        uTables.Add(attachTo.DatabaseName, new List<Type> { type });
=======
                                        namedDbDict.Add(attachTo.DatabaseName, new List<Type> { type });
>>>>>>> 9bfc85f6784b254a10c65f104446a83c8b195c40
                                    }
                                }
                                else if (attachTo.DatabaseType != null)
                                {
<<<<<<< HEAD
                                    if (utTables.TryGetValue(attachTo.DatabaseType, out List<Type> types))
=======
                                    if (typedDbDict.TryGetValue(attachTo.DatabaseType, out List<Type> types))
>>>>>>> 9bfc85f6784b254a10c65f104446a83c8b195c40
=======
                                        namedDb.Add(link.DatabaseName, new List<Type> { type });
                                    }
                                }
                                else if (link.DatabaseType != null)
                                {
                                    if (typedDb.TryGetValue(link.DatabaseType, out List<Type> types))
>>>>>>> premaster
                                    {
                                        types.Add(type);
                                    }
                                    else
                                    {
<<<<<<< HEAD
<<<<<<< HEAD
                                        utTables.Add(attachTo.DatabaseType, new List<Type> { type });
=======
                                        typedDbDict.Add(attachTo.DatabaseType, new List<Type> { type });
>>>>>>> 9bfc85f6784b254a10c65f104446a83c8b195c40
=======
                                        typedDb.Add(link.DatabaseType, new List<Type> { type });
>>>>>>> premaster
                                    }
                                }
                            }
                        }
                    }
                }

<<<<<<< HEAD
<<<<<<< HEAD
                foreach (var pair in uTables)
                {
                    if (TrySearchDatabase(pair.Key, out Database db))
                    {
                        if (db.GetType().GetCustomAttribute<AutoGenerateTables>() == null)
                        {
                            continue;
                        }

                        RegisterTablesTo(db, pair.Value);
                    }
                }
                foreach (var pair in utTables)
                {
                    if (TrySearchDatabase(pair.Key, out Database db))
                    {
                        if (db.GetType().GetCustomAttribute<AutoGenerateTables>() == null)
                        {
                            continue;
                        }

                        RegisterTablesTo(db, pair.Value);
                    }
                }

=======
                foreach (var pair in namedDbDict)
                {
                    TryRegisterDb(pair.Value, dbName: pair.Key);
                }
                foreach (var pair in typedDbDict)
=======
                foreach (var pair in namedDb)
                {
                    TryRegisterDb(pair.Value, dbName: pair.Key);
                }
                foreach (var pair in typedDb)
>>>>>>> premaster
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

<<<<<<< HEAD
                    if (db != null && db.GetType().GetCustomAttribute<AutoGenerateTables>() != null)
=======
                    if (db != null && db.AutoGenerateTable)
>>>>>>> premaster
                    {
                        RegisterTablesTo(db, tables);
                    }
                }
<<<<<<< HEAD
>>>>>>> 9bfc85f6784b254a10c65f104446a83c8b195c40
=======
>>>>>>> premaster
                void RegisterTablesTo(Database db, List<Type> types)
                {
                    foreach (var type in types)
                    {
                        var table = (Table)Activator.CreateInstance(type);
                        db.Create(table);
                    }
<<<<<<< HEAD
<<<<<<< HEAD
=======

                    db.IsInitialized = true;
                    db.OnInitialized();
>>>>>>> 9bfc85f6784b254a10c65f104446a83c8b195c40
=======

                    db.IsInitialized = true;
                    db.OnInitialized();
>>>>>>> premaster
                }
            }
            catch (Exception ex)
            {
                _databases.Clear();
                throw new InitializationFailedException(ex.ToString());
            }
        }
<<<<<<< HEAD
<<<<<<< HEAD
    
=======

        internal static void EnqueueAsyncOperation(BasicAction action)
=======

        internal static void PoolAsyncOperation(BasicAction action)
>>>>>>> premaster
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
<<<<<<< HEAD
        /// <summary>
        /// 
        /// </summary>
        /// <param name="databaseType"></param>
        /// <param name="database"></param>
        /// <returns></returns>
=======
>>>>>>> premaster
        public static bool TrySearchDatabase(Type databaseType, out Database database)
        {
            database = _databases.Values.FirstOrDefault((db) => db.GetType() == databaseType);
            return database != null;
        }

<<<<<<< HEAD
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

>>>>>>> 9bfc85f6784b254a10c65f104446a83c8b195c40
=======
>>>>>>> premaster
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