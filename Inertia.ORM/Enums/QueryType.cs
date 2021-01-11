using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Internal
{
    internal enum QueryType : byte
    {
        CreateTable = 1,
        DeleteTable = 2,
        Insert = 3,
        Update = 4,
        Select = 5,
        Delete = 6,
        Truncate = 7,
        Count = 8
    }
}
