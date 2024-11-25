using System;

namespace Inertia
{
    [AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
    public class DefaultServiceConstructor : Attribute
    {
    }
}