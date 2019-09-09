using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.MySQL
{
    public class QueryAddRangeInvalidCountException : Exception
    {
        public override string Message => GetMessage();
        public readonly Query.QueryBase Query;

        public QueryAddRangeInvalidCountException(Query.QueryBase Query)
        {
            this.Query = Query;
        }

        private string GetMessage()
        {
            return
                "Calling AddRange(string[], object[]) method on the query instance with different array length. fields array and values array need to have the same length";
        }
    }
}
