using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.MySQL
{
    public class UpdateQueryNoSetterException : Exception
    {
        public override string Message => GetMessage();
        public readonly Query.Update Query;

        public UpdateQueryNoSetterException(Query.Update Query)
        {
            this.Query = Query;
        }

        private string GetMessage()
        {
            return
                "No values set on the Query.Update instance, please set values before executing the query.";
        }
    }
}
