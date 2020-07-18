using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.ORM
{
    /// <summary>
    /// Static class allowing several operations
    /// </summary>
    public static class SqlManager
    {
        private static AutoQueue m_queue;

        /// <summary>
        /// Execute an operation asynchronously on a <see cref="Database"/>
        /// </summary>
        /// <typeparam name="T">Database where to execute the operation</typeparam>
        /// <param name="operation">Operation to execute</param>
        /// <param name="onExecuted">Callback called when operation ended with a <see cref="bool"/> parameter (true is success)</param>
        public static void AsyncOperation<T>(SimpleReturnAction<T, object> operation, BasicAction<bool, object> onExecuted = null) where T : Database
        {
            var database = Database.GetDatabase<T>();
            AsyncOperation(database, (db) => operation((T)db), onExecuted);
        }
        /// <summary>
        /// Execute an operation asynchronously on a <see cref="Database"/>
        /// </summary>
        /// <param name="database">Database where to execute the operation</param>
        /// <param name="operation">Operation to execute</param>
        /// <param name="onExecuted">Callback called when operation ended with a <see cref="bool"/> parameter (true is success)</param>
        public static void AsyncOperation(Database database, SimpleReturnAction<Database, object> operation, BasicAction<bool, object> onExecuted = null)
        {
            if (database != null)
            {
                if (m_queue == null)
                    Initialize();

                m_queue.Enqueue(() => {
                    var result = operation(database);

                    if (onExecuted == null)
                        return;

                    var success = result.GetType() == typeof(bool) ? (bool)result : (result != null);
                    onExecuted(success, result);
                });
            }
        }

        private static void Initialize()
        {
            if (m_queue != null)
                return;

            m_queue = new AutoQueue();
        }
    }
}