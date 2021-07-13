using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.ORM
{
    [IgnoreDatabase]
    public sealed class RuntimeDatabase : Database
    {
        public override string Name { get; set; }
        public override string Host { get; set; }
        public override string User { get; set; }
        public override string Password { get; set; }

        internal RuntimeDatabase()
        {
        }
    }
}
