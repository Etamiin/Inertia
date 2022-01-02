using System;

namespace Inertia.ORM
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class PrimaryKey : Attribute
    {
        public bool AutoIncrement { get; private set; }

        public PrimaryKey(bool autoIncrement)
        {
            AutoIncrement = autoIncrement;
        }
    }
}