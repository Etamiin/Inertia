using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.MySQL
{
    public class QueryBrokenException : Exception
    {
        public override string Message => GetMessage();

        public readonly Query.QueryBase Query;

        public QueryBrokenException(Query.QueryBase Query)
        {
            this.Query = Query;
        }

        private string GetMessage()
        {
            return
                "The query is broken, check your MySql connection and your credentials and retry to generate a query.";
        }

    }
}
