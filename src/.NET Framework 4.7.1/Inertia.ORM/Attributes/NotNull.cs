using System;

namespace Inertia.ORM
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class NotNull : Attribute
    {
        public readonly bool Unique;

        public NotNull()
        {
        }
        public NotNull(bool unique)
        {
            Unique = unique;
        }
    }
}
