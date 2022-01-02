using System;

namespace Inertia.ORM
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class DecimalPrecision : Attribute
    {
        public byte FieldPrecision { get; private set; }
        public byte FieldScale { get; private set; }

        public DecimalPrecision()
        {
            FieldPrecision = 12;
            FieldScale = 3;
        }
        public DecimalPrecision(byte precision)
        {
            FieldPrecision = precision;
            FieldScale = 3;
        }
        public DecimalPrecision(byte precision, byte scale)
        {
            FieldPrecision = precision;
            FieldScale = scale;
        }
    }
}
