using System;

namespace Inertia.ORM
{
<<<<<<< HEAD
    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class NotNull : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        public readonly bool Unique;

        /// <summary>
        /// Instantiate a new instance of the class <see cref="NotNull"/>
        /// </summary>
        public NotNull()
        {
        }
        /// <summary>
        /// Instantiate a new instance of the class <see cref="NotNull"/>
        /// </summary>
        /// <param name="unique">Add unique statement to the field ?</param>
=======
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class NotNull : Attribute
    {
        public readonly bool Unique;

        public NotNull()
        {
        }
>>>>>>> premaster
        public NotNull(bool unique)
        {
            Unique = unique;
        }
    }
}
