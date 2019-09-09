using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.MySQL
{
    public class MySqlCredentials
    {
        public readonly string User;
        public readonly string Password;
        public readonly string Database;
        public readonly string Host;

        public MySqlCredentials(string User, string Database) : this(User, "", Database)
        {
        }
        public MySqlCredentials(string User, string Password, string Database) : this(User, Password, Database, "127.0.0.1")
        {
        }
        public MySqlCredentials(string User, string Password, string Database, string Host)
        {
            if (Host == "localhost")
                Host = "127.0.0.1";

            this.User = User;
            this.Password = Password;
            this.Database = Database;
            this.Host = Host;
        }

        public override string ToString()
        {
            return "server=" + Host + ";uid=" + User + ";pwd=" + Password + ";database=" + Database;
        }
    }
}
