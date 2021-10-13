using System;

namespace Inertia.ORM
{
    /// <summary>
    /// Represent the state that will automatically create all tables in a <see cref="Database"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class AutoGenerateTables : Attribute
    {
    }
}
