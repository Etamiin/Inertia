using System;

namespace Inertia.ORM
{
    /// <summary>
    /// Throw when a <see cref="Database"/> is already registered
    /// </summary>
    public class DatabaseAlreadyInitializedException : Exception
    {
        public override string Message => "You can instantiate a Database only once.";

        public readonly string DatabaseName;

        public DatabaseAlreadyInitializedException(string dbName)
        {
            DatabaseName = dbName;
        }
    }
}
