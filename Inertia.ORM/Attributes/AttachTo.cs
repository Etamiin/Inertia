using System;

namespace Inertia.ORM
{
    /// <summary>
    /// Attach a <see cref="Table"/> object to a <see cref="Database"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class AttachTo : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        public readonly string DatabaseName;

        /// <summary>
        /// Instantiate a new instance of the class <see cref="AttachTo"/>
        /// </summary>
        /// <param name="databaseName"></param>
        public AttachTo(string databaseName)
        {
            DatabaseName = databaseName;
        }
    }
}
