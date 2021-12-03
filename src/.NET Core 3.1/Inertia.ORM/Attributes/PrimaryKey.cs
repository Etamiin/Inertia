using System;

namespace Inertia.ORM
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class PrimaryKey : Attribute
    {
        public readonly bool AutoIncrement;

        public PrimaryKey(bool autoIncrement)
        {
            AutoIncrement = autoIncrement;
        }
    }
}
