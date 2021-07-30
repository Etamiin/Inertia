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

        /// <summary>
        /// Returns a <see cref="Database"/> already registered.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Database TrySearchDatabase(string name)
        {
            if (!m_initialized) Initialize();

            m_databases.TryGetValue(name, out Database database);
            return database;
        }
        /// <summary>
        /// Returns a <typeparamref name="T"/> already registered.
        /// </summary>
        /// <returns></returns>
        public static T TrySearchDatabase<T>() where T : Database
        {
            if (!m_initialized) Initialize();

            return (T)m_databases.Values.FirstOrDefault((db) => db.GetType() == typeof(T));
        }

        private static void Initialize()
        {
            m_initialized = true;

            if (m_databases == null) m_databases = new Dictionary<string, Database>();

            var uTables = new Dictionary<string, List<Type>>();

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
                            Database db = null;
                            try
                            {
                                db = (Database)Activator.CreateInstance(type);
                            }
                            catch { }

                            if (m_databases.ContainsKey(db.Name))
                                throw new DatabaseAlreadyInitializedException(db.Name);

                            m_databases.Add(db.Name, db);
                        }
                        else if (type.IsSubclassOf(typeof(Table)))
                        {
                            var attachTo = type.GetCustomAttribute<AttachTo>(false);
                            if (attachTo != null)
                            {
                                var lExist = uTables.TryGetValue(attachTo.DatabaseName, out List<Type> types);
                                
                                if (!lExist) uTables.Add(attachTo.DatabaseName, new List<Type>());
                                uTables[attachTo.DatabaseName].Add(type);
                            }
                        }
                    }
                }

                foreach (var pair in uTables)
                {
                    if (m_databases.TryGetValue(pair.Key, out Database db))
                    {
                        if (db.GetType().GetCustomAttribute<AutoGenerateTables>() == null)
                            continue;

                        foreach (var type in pair.Value)
                        {
                            try
                            {
                                var table = (Table)Activator.CreateInstance(type);
                                db.Create(table);
                            }
                            catch { }
                        }
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
            command.Prepare();

            if (condition != null)
                condition.ApplyToCmd(command);
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