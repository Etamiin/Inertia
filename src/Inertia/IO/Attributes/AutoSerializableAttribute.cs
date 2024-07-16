using System;
using System.Collections.Generic;
using System.Text;

namespace Inertia.IO
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class AutoSerializableAttribute : Attribute
    {
    }
}
