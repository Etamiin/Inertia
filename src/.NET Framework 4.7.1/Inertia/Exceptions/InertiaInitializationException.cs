using System;
using System.Collections.Generic;
using System.Text;

namespace Inertia
{
    /// <summary>
    /// This exception is thrown in case an error occurs during the initialization of a core component.
    /// </summary>
    public class InertiaInitializationException : Exception
    {
        public readonly string ComponentName;
        public readonly Exception GeneratedException;

        public InertiaInitializationException(string componentName, Exception ex)
        {
            ComponentName = componentName;
            GeneratedException = ex;
        }
    }
}
