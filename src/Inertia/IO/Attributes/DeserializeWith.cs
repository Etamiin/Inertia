using System;
using System.Collections.Generic;
using System.Text;

namespace Inertia
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class DeserializeWith : Attribute
    {
        internal readonly string MethodName;

        public DeserializeWith(string methodName)
        {
            MethodName = methodName;
        }
    }
}
