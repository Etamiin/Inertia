using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Inertia.Internal;
using MySql.Data.MySqlClient;

namespace Inertia.ORM
{
    /// <summary>
    /// Main class for ORM access
    /// </summary>
    public static class SqlManager
    {
        private static AutoQueueExecutor m_queue;
        private static Dictionary<string, Database> m_databases;
        private static Dictionary<Type, Table> m_tables;
        private static Random m_random;
        private readonly static char[] m_chars = new char[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

        internal static object CreateInstance(Type targetType)
        {
            var constructor = targetType.GetConstructor(Type.EmptyTypes);
            if (constructor == null)
            {
                var constructors = targetType.GetConstructors();
                if (constructors.Length == 0)
                    return null;

                var objs = new object[constructors[0].GetParameters().Length];
                for (var i = 0; i < objs.Length; i++)
                    objs[i] = null;

                return constructor.Invoke(objs);
            }
            else
                return constructor.Invoke(new object[] { });
        }
        internal static Table[] GetTablesFromDatabase(Database database)
        {
            var tables = new List<Table>();

            foreach (var table in m_tables.Values)
            {
                if (table.Database.GetType() == database.GetType())
                    tables.Add(table);
            }

            return tables.ToArray();
        }

        internal static string GenerateRandomName()
        {
            if (m_random == null)
                m_random = new Random();

            var n = "@";
            for (var i = 0; i < 7; i++)
                n += m_chars[m_random.Next(0, m_chars.Length)];

            return n;
        }

        /// <summary>
        /// Generate all <see cref="Table"/> in <see cref="Database"/>
        /// </summary>
        public static void ManualTablesGeneration()
        {
            if (m_databases == null)
                LoadDatabases();

            foreach (var db in m_databases)
                db.Value.CreateAllTables();
        }

        internal static bool ExecuteQuery<T>(SqlQuery<T> query, out int result) where T : Table
        {
            result = -1;

            try
            {
                query.Command.CommandText = query.GetQuery();
                result = query.Command.ExecuteNonQuery();

                return true;
            }
            catch (Exception ex) {
                query.GetLogger().Log(ex.Message);
                return false;
            }
        }
        internal static void ExecuteQueryAsync<T>(SqlQuery<T> query, BasicAction<bool, int> callback) where T : Table
        {
            m_queue.Enqueue(() => {
                var success = ExecuteQuery(query, out int result);
                callback(success, result);
            });
        }

        /// <summary>
        /// Get a <see cref="Database"/> instance based on the name
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
        /// Get a <see cref="Database"/> instance based on the <see cref="Type"/>
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
        /// Get a <see cref="Database"/> instance based on the <see cref="Type"/>
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
        /// Get a <see cref="Table"/> instance based on the <see cref="Type"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetTable<T>() where T : Table
        {
            if (m_tables == null)
                LoadDatabases();

            m_tables.TryGetValue(typeof(T), out Table table);
            return (T)table;
        }

        /// <summary>
        /// Get the specified <typeparamref name="T"/> instance and execute <paramref name="usage"/> action
        /// </summary>
        /// <typeparam name="T"><see cref="Type"/> of <see cref="Database"/> to get</typeparam>
        /// <param name="usage">Action to execute</param>
        public static void UseDb<T>(BasicAction<T> usage) where T : Database
        {
            var db = GetDatabase<T>();
            if (db != null)
                usage(db);
        }
        /// <summary>
        /// Get the specified <typeparamref name="T"/> instance and execute usage action
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="usage"></param>
        public static void UseTable<T>(BasicAction<T> usage) where T : Table
        {
            var table = GetTable<T>();
            if (table != null)
                usage(table);
        }

        private static void LoadDatabases()
        {
            if (m_queue == null)
                m_queue = new AutoQueueExecutor();

            m_databases = new Dictionary<string, Database>();
            m_tables = new Dictionary<Type, Table>();

            var tables = new List<Type>();
            var assemblys = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblys)
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (type.IsAbstract)
                        continue;

                    if (type.IsClass)
                    {
                        if (type.IsSubclassOf(typeof(Database)))
                        {
                            var autoCreateAttr = type.GetCustomAttribute<AutoGenerateTables>();
                            var database = (Database)CreateInstance(type);
                            if (database != null && !m_databases.ContainsKey(database.Name))
                            {
                                database.AutoCreateTables = autoCreateAttr != null;
                                m_databases.Add(database.Name, database);
                            }
                        }
                        else if (type.IsSubclassOf(typeof(Table)))
                            tables.Add(type);
                    }
                }
            }

            foreach (var tableType in tables)
            {
                var table = (Table)CreateInstance(tableType);
                if (table != null && table.Database != null)
                    m_tables.Add(tableType, table);
            }

            foreach (var db in m_databases.Values)
            {
                if (db.AutoCreateTables)
                    db.CreateAllTables();

                db.OnCreated();
            }
        }
    }
}