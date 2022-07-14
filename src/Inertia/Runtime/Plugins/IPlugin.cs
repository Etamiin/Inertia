using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Runtime
{
    [Obsolete]
    public interface IPlugin
    {
        void OnInitialize();
        void OnExecute();
    }
}
