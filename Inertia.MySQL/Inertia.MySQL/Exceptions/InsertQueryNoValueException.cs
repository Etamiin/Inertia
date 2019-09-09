using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.MySQL
{
    public class InsertQueryNoValueException : Exception
    {
        public override string Message => GetMessage();
        public readonly Query.Insert Query;

        public InsertQueryNoValueException(Query.Insert Query)
        {
            this.Query = Query;
        }

        private string GetMessage()
        {
            return
                "No values set on the Query.Insert instance, please set values before executing the query.";
        }
    }
}
