using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.ORM
{
    public abstract class Database
    {
        #region Static variables

        private static Dictionary<string, Database> _databases;

        #endregion

        #region Static methods

        private static void LoadDatabases()
        {
            _databases = new Dictionary<string, Database>();

            var Assemblys = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in Assemblys)
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (type.IsClass && type.IsSubclassOf(typeof(Database)))
                    {
                        var database = (Database)type.GetConstructor(Type.EmptyTypes).Invoke(new object[] { });
                        if (GetDatabase(database.Name) == null)
                            _databases.Add(database.Name, database);
                    }
                }
            }

            Table.LoadTables();
            Table.CreateTables(_databases.Values.ToArray());
        }
        
        public static T GetDatabase<T>(string name) where T : Database
        {
            var database = GetDatabase(name);
            if (database != null)
                return (T)database;
            return null;
        }
        public static Database GetDatabase(string name)
        {
            if (_databases == null)
                LoadDatabases();

            _databases.TryGetValue(name, out Database database);
            return database;
        }

        public static void UseDatabase(string databaseName, OrmActionHandler<Database> usageAction)
        {
            UseDatabase<Database>(databaseName, usageAction);
        }
        public static void UseDatabase<T>(string databaseName, OrmActionHandler<T> usageAction) where T : Database
        {
            var database = GetDatabase(databaseName);
            if (database != null)
                usageAction((T)database);
        }
        public static void UseTable(string databaseName, string tableName, OrmActionHandler<Table> usageAction)
        {
            UseTable<Table>(databaseName, tableName, usageAction);
        }
        public static void UseTable<T>(string databaseName, string tableName, OrmActionHandler<T> usageAction) where T : Table
        {
            var database = GetDatabase(databaseName);
            if (database != null)
            {
                var table = database.GetTable(tableName);
                if (table != null)
                    usageAction((T)table);
            }
        }

        #endregion

        #region Public variables

        public abstract string Name { get; }
        public abstract string Host { get; }
        public abstract string User { get; }
        public abstract string Password { get; }

        #endregion

        #region Private variables

        private readonly Dictionary<string, Table> Tables;

        #endregion

        #region Constructors

        public Database()
        {
            Tables = new Dictionary<string, Table>();
        }

        #endregion

        public Table[] GetTables()
        {
            return Tables.Values.ToArray();
        }

        public T GetTable<T>(string name) where T : Table
        {
            var table = GetTable(name);
            if (table != null)
                return (T)table;
            return null;
        }
        public Table GetTable(string name)
        {
            Tables.TryGetValue(name, out Table table);
            return table;
        }

        public MySqlConnection CreateConnection()
        {
            var conn = new MySqlConnection("server=" + Host + ";uid=" + User + ";pwd=" + Password + ";database=" + Name);
            conn.Open();

            return conn;
        }
        public MySqlCommand CreateCommand()
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

        internal void AddTable(Table table)
        {
            if (GetTable(table.Name) == null)
                Tables.Add(table.Name, table);
        }
    }
}
