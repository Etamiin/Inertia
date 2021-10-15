using System;
using System.Collections.Generic;
using System.Text;

namespace Inertia
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class CustomDeserialization : Attribute
    {
        internal readonly string MethodName;

        /// <summary>
        /// Specify that the field with a <see cref="CustomDeserialization"/> attribute will execute the specfied method to be deserialized
        /// </summary>
        /// <param name="methodName"></param>
        public CustomDeserialization(string methodName)
        {
            MethodName = methodName;
        }
    }
}
