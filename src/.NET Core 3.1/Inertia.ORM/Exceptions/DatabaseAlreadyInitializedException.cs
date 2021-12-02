using System;

namespace Inertia.ORM
{
    /// <summary>
    /// Throw when a <see cref="Database"/> is already registered
    /// </summary>
    public class DatabaseAlreadyInitializedException : Exception
    {
<<<<<<< HEAD
        /// <summary>
        ///
        /// </summary>
        public override string Message => "You can instantiate a Database only once.";

        /// <summary>
        ///
        /// </summary>
        public readonly string DatabaseName;

        /// <summary>
        /// Instantiate a new instance of the class <see cref="DatabaseAlreadyInitializedException"/>
        /// </summary>
        /// <param name="dbName"></param>
=======
        public override string Message => "You can instantiate a Database only once.";

        public readonly string DatabaseName;

>>>>>>> premaster
        public DatabaseAlreadyInitializedException(string dbName)
        {
            DatabaseName = dbName;
        }
    }
}
