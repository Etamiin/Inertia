using System;
using System.Collections.Generic;
using System.Text;

namespace Inertia
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class CustomDeserialization : Attribute
    {
        internal readonly string MethodName;

        public CustomDeserialization(string methodName)
        {
            MethodName = methodName;
        }
    }
}
