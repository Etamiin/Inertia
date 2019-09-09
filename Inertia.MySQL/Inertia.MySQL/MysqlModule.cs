using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.MySQL
{
    public class MySqlModule
    {
        private static MySqlModule instance;
        public static MySqlModule Module
        {
            get
            {
                if (instance == null)
                    instance = new MySqlModule();
                return instance;
            }
        }

        public MySqlCredentials Credentials;

        internal MySqlModule()
        {
        }

        public MySqlConnection CreateConnection()
        {
            if (Credentials == null)
                throw new NullCredentialsException();

            MySqlConnection conn = null;

            try
            {
                conn = new MySqlConnection(Credentials.ToString());
                conn.Open();
            }
            catch (MySqlException ex) {
                Logger.Error(ex);
                conn = null;
            }

            return conn;
        }
        public MySqlCommand CreateCommand()
        {
            var cmd = new MySqlCommand {
                Connection = CreateConnection()
            };

            if (cmd.Connection == null)
                return null;

            cmd.Prepare();
            return cmd;
        }
    }
}
