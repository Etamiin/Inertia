using System;

namespace Inertia.ORM
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class NotNull : Attribute
    {
        public bool Unique { get; private set; }

        public NotNull()
        {
        }
        public NotNull(bool unique)
        {
            Unique = unique;
        }
    }
}
