using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inertia.Scripting;

namespace Inertia.Internal
{
    internal class Updater : IDisposable
    {
        #region Events

        private event InertiaAction UpdateHandler = () => { };

        #endregion

        #region Public variables

        public int Count { get; private set; }

        #endregion

        #region Internal variables

        internal bool IsDisposed { get; private set; }

        #endregion

        public void AddHandler(InertiaAction update)
        {
            if (IsDisposed)
                return;

            UpdateHandler += update;
            Count++;
        }
        public void RemoveHandler(InertiaAction update)
        {
            UpdateHandler -= update;
            
            if (--Count == 0)
                Dispose();
        }
        
        public void Execute()
        {
            if (IsDisposed)
                return;

            UpdateHandler?.Invoke();
        }
        public void Dispose()
        {
            IsDisposed = true;
            UpdateHandler = null;
            ScriptingManager.RemoveContainer(this);
        }
    }
}
