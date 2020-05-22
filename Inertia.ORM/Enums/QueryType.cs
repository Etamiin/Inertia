using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Internal
{
    internal enum QueryType
    {
        Delete,
        DeleteWithAutoCondition,
        DeleteAll,
        Insert,
        Select,
        Update,
        CreateTable,
        DropTable,
        Count
    }
}
