using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace Inertia
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class CustomSerialization : Attribute
    {
        internal readonly string MethodName;

        public CustomSerialization(string methodName)
        {
            MethodName = methodName;
        }
    }
}
