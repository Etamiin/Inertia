using System;

namespace Inertia.ORM
{
    /// <summary>
    /// Throw when failed to connect to a <see cref="Database"/>
    /// </summary>
    public class DatabaseConnectionFailedException : Exception
    {
        /// <summary>
        ///
        /// </summary>
        public override string Message => $"Can't connect to database '{ Database.Name }'. Invalid credentials?";

        /// <summary>
        /// Database that throwed the exception
        /// </summary>
        public readonly Database Database;
        /// <summary>
        /// 
        /// </summary>
        public readonly Exception Exception;

        /// <summary>
        /// Instantiante a new instance of the class <see cref="DatabaseConnectionFailedException"/>
        /// </summary>
        /// <param name="database"></param>
        /// <param name="ex"></param>
        public DatabaseConnectionFailedException(Database database, Exception ex)
        {
            Database = database;
            Exception = ex;
        }
    }
}
