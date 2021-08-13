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
        private static Dictionary<string, Database> m_databases;

        private static bool m_initialized;
        private static AutoQueueExecutor m_queue;

        internal static void EnqueueAsyncOperation(BasicAction action)
        {
            m_queue.Enqueue(action);
        }

        /// <summary>
        /// Returns a <see cref="Database"/> already registered.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="database"></param>
        /// <returns></returns>
        public static bool TrySearchDatabase(string name, out Database database)
        {
            if (!m_initialized) Initialize();

            return m_databases.TryGetValue(name, out database);
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
        /// Returns a <typeparamref name="T"/> already registered.
        /// </summary>
        /// <returns></returns>
        public static bool TrySearchDatabase(Type databaseType, out Database database)
        {
            if (!m_initialized) Initialize();

            database = m_databases.Values.FirstOrDefault((db) => db.GetType() == databaseType);
            return database != null;
        }

        /// <summary>
        /// Try to find the specified <typeparamref name="T"/> <see cref="Database"/> and execute the specified action with it
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="onDatabase"></param>
        public static void TryUseDatabase<T>(BasicAction<T> onDatabase) where T : Database
        {
            if (TrySearchDatabase<T>(out T db))
                onDatabase?.Invoke(db);
        }

        private static void Initialize()
        {
            m_initialized = true;

            if (m_databases == null) m_databases = new Dictionary<string, Database>();
            if (m_queue == null) m_queue = new AutoQueueExecutor();

            var uTables = new Dictionary<string, List<Type>>();
            var utTables = new Dictionary<Type, List<Type>>();

            try
            {
                var assemblys = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assemblys)
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (type.IsAbstract || !type.IsClass)
                            continue;

                        if (type.IsSubclassOf(typeof(Database)))
                        {
                            var db = (Database)Activator.CreateInstance(type);
                            if (m_databases.ContainsKey(db.Name))
                                throw new DatabaseAlreadyInitializedException(db.Name);

                            db.TryCreateItSelf();
                            m_databases.Add(db.Name, db);
                        }
                        else if (type.IsSubclassOf(typeof(Table)))
                        {
                            var attachTo = type.GetCustomAttribute<AttachTo>(false);
                            if (attachTo != null)
                            {
                                if (!string.IsNullOrEmpty(attachTo.DatabaseName))
                                {
                                    if (uTables.TryGetValue(attachTo.DatabaseName, out List<Type> types))
                                        types.Add(type);
                                    else
                                        uTables.Add(attachTo.DatabaseName, new List<Type>() { type });
                                }
                                else if (attachTo.DatabaseType != null)
                                {
                                    if (utTables.TryGetValue(attachTo.DatabaseType, out List<Type> types))
                                        types.Add(type);
                                    else
                                        utTables.Add(attachTo.DatabaseType, new List<Type>() { type });
                                }
                            }
                        }
                    }
                }

                foreach (var pair in uTables)
                {
                    if (TrySearchDatabase(pair.Key, out Database db))
                    {
                        if (db.GetType().GetCustomAttribute<AutoGenerateTables>() == null)
                            continue;

                        RegisterTablesTo(db, pair.Value);
                    }
                }
                foreach (var pair in utTables)
                {
                    if (TrySearchDatabase(pair.Key, out Database db))
                    {
                        if (db.GetType().GetCustomAttribute<AutoGenerateTables>() == null)
                            continue;

                        RegisterTablesTo(db, pair.Value);
                    }
                }

                void RegisterTablesTo(Database db, List<Type> types)
                {
                    foreach (var type in types)
                    {
                        var table = (Table)Activator.CreateInstance(type);
                        db.Create(table);
                    }
                }
            }
            catch (Exception ex)
            {
                m_databases.Clear();
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
            if (condition != null) condition.ApplyToCmd(command);

            command.Prepare();
        }
        internal static void OnReader(this MySqlCommand command, BasicAction<MySqlDataReader> onReader)
        {
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                    onReader(reader);
            }
        }
    }
}