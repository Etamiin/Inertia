using System;

namespace Inertia.ORM
{
    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class DecimalPrecision : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        public readonly byte FieldPrecision;
        /// <summary>
        /// 
        /// </summary>
        public readonly byte FieldScale;

        /// <summary>
        /// Instantiate a new instance of the class <see cref="DecimalPrecision"/>
        /// </summary>
        /// <param name="precision"></param>
        /// <param name="scale"></param>
        public DecimalPrecision(byte precision = 12, byte scale = 3)
        {
            FieldPrecision = precision;
            FieldScale = scale;
        }
    }
}
