using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.MySQL
{
    public class DeleteQuerySecureException : Exception
    {
        public override string Message => GetMessage();
        public readonly Query.Delete Query;
        public readonly bool SecurityNeeded;

        public DeleteQuerySecureException(Query.Delete Query, bool SecurityNeeded)
        {
            this.Query = Query;
            this.SecurityNeeded = SecurityNeeded;
        }

        private string GetMessage()
        {
            if (SecurityNeeded)
                return "You need to use the DeleteSecure() method from the Query.Delete instance because no condition was set.";
            else
                return "You need to use the Delete() method from the Query.Delete instance because conditions was set, no confirmation needed.";
        }
    }
}
