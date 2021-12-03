using System;

namespace Inertia.ORM
{
    /// <summary>
    /// Throw when failed to connect to a <see cref="_database"/>
    /// </summary>
    public class DatabaseConnectionFailedException : Exception
    {
        public override string Message => $"Can't connect to database '{ _database.Name }': { _exception }";

        private readonly Database _database;
        private readonly Exception _exception;

        public DatabaseConnectionFailedException(Database database, Exception ex)
        {
            _database = database;
            _exception = ex;
        }
    }
}
