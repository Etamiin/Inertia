using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia
{
    /// <summary>
    /// 
    /// </summary>
    public interface IInertiaPlugin
    {
        /// <summary>
        /// Initializaiton method for the plugin
        /// </summary>
        void OnInitialize();
        /// <summary>
        /// Execution method for the plugin
        /// </summary>
        void OnExecute();
    }
}
