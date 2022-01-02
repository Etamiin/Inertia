using System;

namespace Inertia.ORM
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class VarChar : Attribute
    {
        public int Length => _length;

        private int _length;

        public VarChar(int length)
        {
            _length = length;
        }
    }
}
