using System;
using System.Collections.Generic;
using System.Text;

namespace Inertia
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class DeserializeWith : Attribute
    {
        internal readonly string MethodName;

        public DeserializeWith(string methodName)
        {
            MethodName = methodName;
        }
    }
}
