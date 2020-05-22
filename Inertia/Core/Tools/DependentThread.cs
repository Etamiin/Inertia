using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Inertia
{
    /// <summary>
    /// Offers the possibility to asynchronously execute an action under the dependency of a <see cref="Thread"/>
    /// </summary>
    public static class DependentThread
    {
        /// <summary>
        /// Asynchronously executes an action in a new <see cref="Thread"/> as long as the parent <see cref="Thread"/> is executing
        /// </summary>
        /// <param name="executionCode">The code to continually execute (loop)</param>
        /// <param name="onDependencyDied">Code to execute when the dependency is ended</param>
        /// <param name="additionalConditions">The additional conditions to process the specified code</param>
        public static void ExecuteThreadWhileDependencyIsAlive(SimpleAction executionCode, SimpleAction onDependencyDied = null, SimpleReturnAction<bool> additionalConditions = null)
        {
            var dependency = Thread.CurrentThread;
            var thread = new Thread(() =>
            {
                while (dependency.IsAlive)
                {
                    if (additionalConditions != null && !additionalConditions())
                        break;

                    executionCode();
                }

                onDependencyDied?.Invoke();
            });

            thread.Start();
        }
        /// <summary>
        /// Asynchronously executes an action in a new <see cref="Task"/> as long as the parent <see cref="Thread"/> is executing
        /// </summary>
        /// <param name="executionCode">The code to continually execute (loop)</param>
        /// <param name="onDependencyDied">Code to execute when the dependency is ended</param>
        /// <param name="additionalConditions">The additional conditions to process the specified code</param>
        public static void ExecuteTaskWhileDependencyIsAlive(SimpleAction executionCode, SimpleAction onDependencyDied = null, SimpleReturnAction<bool> additionalConditions = null)
        {
            var dependency = Thread.CurrentThread;
            Task.Factory.StartNew(() => {
                while (dependency.IsAlive)
                {
                    if (additionalConditions != null && !additionalConditions())
                        break;

                    executionCode();
                }

                onDependencyDied?.Invoke();
            });
        }
    }
}
