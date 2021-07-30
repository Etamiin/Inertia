using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inertia.ORM;

namespace InertiaTests.ORM
{
    [AttachTo("local")]
    public class UserTable : Table
    {
        public override string Identifier => "users";

        [PrimaryKey(true)]
        public int id;
        [VarChar(60)]
        public string username;
        public string password;
        public DateTime birthday;
        [DecimalPrecision]
        public decimal randomValue;
        public double randomDouble;
        public float randomFloat;
    }
}
