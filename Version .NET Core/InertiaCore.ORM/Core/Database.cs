using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Inertia.Internal;
using System.Diagnostics;

namespace Inertia.ORM
{
    /// <summary>
    /// Represent the database class
    /// </summary>
    public abstract class Database
    {
        /// <summary>
        /// Create a <see cref="RuntimeDatabase"/> with specified informations
        /// </summary>
        /// <param name="name"></param>
        /// <param name="host"></param>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <param name="onCreated"></param>
        /// <param name="port"></param>
        /// <param name="autoGenerate"></param>
        /// <returns></returns>
        public static Database Create(string name, string host, string user, string password, BasicAction onCreated, int port = 3306, bool autoGenerate = false)
        {
            var db = new RuntimeDatabase()
            {
                Name = name,
                Host = host,
                User = user,
                Password = password,
                Port = port,
                AutoCreateTables = autoGenerate
            };

            SqlManager.RegisterCustomDatabase(db);
            if (db.AutoCreateTables)
                db.CreateAllTables();

            onCreated?.Invoke();
            return db;
        }

        #region Public variables

        /// <summary>
        /// Get the name of the database
        /// </summary>
        public abstract string Name { get; set; }
        /// <summary>
        /// Get the host ip of the database
        /// </summary>
        public abstract string Host { get; set; }
        /// <summary>
        /// Get the username to use for the connection
        /// </summary>
        public abstract string User { get; set; }
        /// <summary>
        /// Get the password to use for the connection
        /// </summary>
        public abstract string Password { get; set; }
        /// <summary>
        /// Get the port to use for the connection
        /// </summary>
        public virtual int Port { get; set; } = 3306;

        #endregion

        #region Internal variables

        internal bool AutoCreateTables;

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        public Database()
        {
        }

        #endregion

        /// <summary>
        /// Happens one time after the initialization (after creating <see cref="Table"/>)
        /// </summary>
        public virtual void OnCreated() { }

        internal MySqlConnection CreateConnection()
        {
            try
            {
                var conn = new MySqlConnection("server=" + Host + ";uid=" + User + ";pwd=" + Password + ";database=" + Name + ";port=" + Port);
                conn.Open();

                return conn;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                this.GetLogger().Log("Can't connect to MySql-server: " + ex);
                return null;
            }
        }
        internal MySqlCommand CreateCommand()
        {
            var command = new MySqlCommand() { Connection = CreateConnection() };
            if (command.Connection == null)
                return null;

            command.Prepare();
            return command;
        }

        /// <summary>
        /// Get a <see cref="Table"/> instance based on the <see cref="Type"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetTable<T>() where T : Table
        {
            return SqlManager.GetTable<T>();
        }

        /// <summary>
        /// Create all tables of the current <see cref="Database"/>
        /// </summary>
        public void CreateAllTables()
        {
            var tables = SqlManager.GetTablesFromDatabase(this);
            foreach (var table in tables)
                table.Create();
        }
    }
}
