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
        /// 
        /// </summary>
        void OnInitialize();
        /// <summary>
        /// 
        /// </summary>
        void OnExecute();
    }
}
