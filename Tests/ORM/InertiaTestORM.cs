using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inertia.ORM;

namespace InertiaTests.ORM
{
    public class InertiaTestORM
    {
        public InertiaTestORM()
        {
            var db = SqlManager.TrySearchDatabase<LocalDatabase>();

            db.DoStuff();
        }

    }
}
