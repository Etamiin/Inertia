using System;

namespace Inertia.ORM
{
    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class IgnoreField : Attribute
    {
        /// <summary>
        /// Instantiate a new instance of the class <see cref="IgnoreField"/>
        /// </summary>
        public IgnoreField()
        {
        }
    }
}
