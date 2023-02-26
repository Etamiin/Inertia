using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace Inertia
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class SerializeWith : Attribute
    {
        internal readonly string MethodName;

        public SerializeWith(string methodName)
        {
            MethodName = methodName;
        }
    }
}
