using System;

namespace Inertia.ORM
{
<<<<<<< HEAD
    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class PrimaryKey : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        public readonly bool AutoIncrement;

        /// <summary>
        /// Instantiate a new instance of the class <see cref="PrimaryKey"/>
        /// </summary>
        /// <param name="autoIncrement">Set the auto increment state of the primary key field</param>
=======
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class PrimaryKey : Attribute
    {
        public readonly bool AutoIncrement;

>>>>>>> premaster
        public PrimaryKey(bool autoIncrement)
        {
            AutoIncrement = autoIncrement;
        }
    }
}
