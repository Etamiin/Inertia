using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.MySQL
{
    public class MysqlModule
    {
        public static class Config
        {
            public static string Host { get; private set; }
            public static string User { get; private set; }
            public static string Pass { get; private set; }
            public static string Database { get; private set; }

            public static string ConnStr
            {
                get
                {
                    return "server=" + Host + ";uid=" + User + ";pwd=" + Pass + ";database=" + Database;
                }
            }

            public static void Set(string User, string Database, string Pass = "", string Host = "127.0.0.1")
            {
                Config.User = User;
                Config.Database = Database;
                Config.Pass = Pass;
                Config.Host = Host;
            }
        }

        private static MysqlModule instance;
        public static MysqlModule Module
        {
            get
            {
                if (instance == null)
                    instance = new MysqlModule();
                return instance;
            }
        }

        internal MysqlModule()
        {

        }

        public MySqlConnection CreateConnection()
        {
            MySqlConnection conn = null;

            try
            {
                conn = new MySqlConnection(Config.ConnStr);
                conn.Open();
            }
            catch (MySqlException ex) {
                InDebug.Error(ex);
                conn = null;
            }

            return conn;
        }
        public MySqlCommand CreateCommand()
        {
            var cmd = new MySqlCommand
            {
                Connection = CreateConnection()
            };

            if (cmd.Connection == null)
                return null;

            cmd.Prepare();
            return cmd;
        }
    }
}
