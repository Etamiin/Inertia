using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.MySQL
{
    public class NullCredentialsException : Exception
    {
        public override string Message => GetMessage();

        public NullCredentialsException()
        {
        }

        private string GetMessage()
        {
            return
                "Query not executed, no credentials set on" + nameof(MySqlModule) + " class, no connection can be etablished" +
                " please set a new " + nameof(MySqlCredentials) + " on " + nameof(MySqlModule) + ".Credentials propertie";
        }
    }
}
