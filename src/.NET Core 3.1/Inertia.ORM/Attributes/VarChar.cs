using System;

namespace Inertia.ORM
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class VarChar : Attribute
    {
        public readonly int Length;

        public VarChar(int length)
        {
            Length = length;
        }
    }
}
