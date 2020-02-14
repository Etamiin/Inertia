using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Web
{
    public class WebModuleConfiguration : IDisposable
    {
        #region Public variables

        public bool StopOnFail = true;
        public int MaxRetryAttempt = 5;

        #endregion

        public void Dispose()
        {
        }
    }
}
