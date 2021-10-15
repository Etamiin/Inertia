using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Inertia
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class CustomSerialization : Attribute
    {
        internal readonly string MethodName;

        /// <summary>
        /// Specify that the field with a <see cref="CustomSerialization"/> attribute will execute the specfied method to be serialized
        /// </summary>
        /// <param name="methodName"></param>
        public CustomSerialization(string methodName)
        {
            MethodName = methodName;
        }
    }
}
