using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia
{
    /// <summary>
    /// Represent a plugin
    /// </summary>
    public abstract class InertiaPlugin
    {
        /// <summary>
        /// Initializaiton method for the plugin
        /// </summary>
        public abstract void OnInitialize();
        /// <summary>
        /// Execution method for the plugin (after all plugins are initialized)
        /// </summary>
        public abstract void OnExecute();
    }
}
