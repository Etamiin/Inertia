using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inertia.Scripting;

namespace Inertia.Internal
{
    internal class ExecuteInScript : Scriptable
    {
        #region Public variables

        public float Time { get; internal set; }

        #endregion

        #region Internal variables

        internal InertiaAction Action;
        
        #endregion

        #region Private variables

        private Clock Clock;

        #endregion

        protected override void Start()
        {
            Clock = Clock.Create();
        }
        protected override void Update()
        {
            if (Action == null)
                return;

            if (Clock.GetElapsedSeconds() >= Time) {
                Action();
                Clock.Dispose();
                Destroy();
            }
        }
    }
}
