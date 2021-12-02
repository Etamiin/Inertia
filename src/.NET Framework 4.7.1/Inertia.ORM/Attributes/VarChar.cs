using System;

namespace Inertia.ORM
{
<<<<<<< HEAD
    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class VarChar : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        public readonly int Length;

        /// <summary>
        /// Instantiate a new instance of the class <see cref="VarChar"/>
        /// </summary>
        /// <param name="length"></param>
=======
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class VarChar : Attribute
    {
        public readonly int Length;

>>>>>>> premaster
        public VarChar(int length)
        {
            Length = length;
        }
    }
}
