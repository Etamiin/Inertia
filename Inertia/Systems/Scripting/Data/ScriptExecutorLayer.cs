using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inertia.Realtime;

namespace Inertia.Internal
{
    internal class ScriptExecutorLayer
    {
        #region Events

        public event BasicAction Executor = () => { };

        #endregion

        #region Internal variables

        internal int Count { get; private set; }
        internal bool IsDisposed { get; private set; }
        internal bool LimitAchieved
        {
            get
            {
                return Count >= RealtimeManager.MaxExecutorScriptCount;
            }
        }

        #endregion

        public void Join(Script script)
        {
            if (IsDisposed)
                return;

            script.ExecutorLayer = this;

            Executor += script.Update;
            Count++;
        }
        public void Leave(Script script)
        {
            if (IsDisposed)
                return;

            script.ExecutorLayer = null;

            Executor -= script.Update;
            Count--;
        }

        public void Execute()
        {
            if (IsDisposed)
                return;

            lock (Executor)
                Executor?.Invoke();
        }

        public void Dispose()
        {
            if (IsDisposed)
                return;

            IsDisposed = true;
            Executor = null;
        }
    }
}
