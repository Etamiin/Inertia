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
        public string ComponentName => _component;
        public Exception Exception => _ex;
        
        private string _component;
        private Exception _ex;

        public InertiaInitializationException(string componentName, Exception ex)
        {
            _component = componentName;
            _ex = ex;
        }
    }
}
