using System;

namespace Inertia.ORM
{
    /// <summary>
    /// Throw when a <see cref="Database"/> is already registered
    /// </summary>
    public class DatabaseAlreadyInitializedException : Exception
    {
        public override string Message => $"You can instantiate a Database only once (Name: {_dbName}).";

        private string _dbName;

        public DatabaseAlreadyInitializedException(string dbName)
        {
            _dbName = dbName;
        }
    }
}
