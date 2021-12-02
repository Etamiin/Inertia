using System;

namespace Inertia.ORM
{
<<<<<<< HEAD
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
=======
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class DecimalPrecision : Attribute
    {
        public readonly byte FieldPrecision;
        public readonly byte FieldScale;

>>>>>>> premaster
        public DecimalPrecision()
        {
            FieldPrecision = 12;
            FieldScale = 3;
        }
<<<<<<< HEAD
        /// <summary>
        /// Instantiate a new instance of the class <see cref="DecimalPrecision"/>
        /// </summary>
        /// <param name="precision"></param>
=======
>>>>>>> premaster
        public DecimalPrecision(byte precision)
        {
            FieldPrecision = precision;
            FieldScale = 3;
        }
<<<<<<< HEAD
        /// <summary>
        /// Instantiate a new instance of the class <see cref="DecimalPrecision"/>
        /// </summary>
        /// <param name="precision"></param>
        /// <param name="scale"></param>
=======
>>>>>>> premaster
        public DecimalPrecision(byte precision, byte scale)
        {
            FieldPrecision = precision;
            FieldScale = scale;
        }
    }
}
