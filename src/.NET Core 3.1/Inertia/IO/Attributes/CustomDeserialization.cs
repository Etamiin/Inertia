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
<<<<<<< HEAD
        /// Specify that the field with a <see cref="CustomDeserialization"/> attribute will execute the specfied method to be deserialized
=======
        /// Specify that the value of the field will be deserialized by the method indicated in parameter.
>>>>>>> premaster
        /// </summary>
        /// <param name="methodName"></param>
        public CustomDeserialization(string methodName)
        {
            MethodName = methodName;
        }
    }
}
