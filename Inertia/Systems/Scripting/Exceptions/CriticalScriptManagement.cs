using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Scripting
{
    public class CriticalScriptManagement : Exception
    {
        #region Public variables

        public override string Message => "Critical error: Too many scripts are running simultaneously";

        #endregion

        #region Constructors

        internal CriticalScriptManagement()
        {
        }

        #endregion
    }
}
