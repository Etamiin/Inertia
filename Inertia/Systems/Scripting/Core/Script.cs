using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inertia.Internal;

namespace Inertia.Realtime
{
    /// <summary>
    /// Execute realtime code
    /// </summary>
    public abstract class Script : IDisposable
    {
        #region Static variables

        /// <summary>
        /// Returns the time elapsed since the last execution of the scripts. 
        /// </summary>
        public static float DeltaTime { get; internal set; }
        /// <summary>
        /// Returns the time elapsed from the process start time
        /// </summary>
        public static float Time { get; internal set; }

        /// <summary>
        /// Get or set the maximum execution per second for scripting system
        /// </summary>
        public static int MaxExecutionPerSecond = 335;

        #endregion

        #region Public variables

        /// <summary>
        /// Return true is the current instance is destroyed
        /// </summary>
        public bool IsDestroyed { get; private set; }

        #endregion

        #region Internal variables

        internal ScriptExecutorLayer ExecutorLayer;
        internal ScriptCollection Collection;
        internal bool IsDisposed { get; private set; }

        #endregion

        #region Private variables

        private bool m_canUpdate;

        #endregion

        internal void Awake(object[] args)
        {
            RealtimeManager.OnScriptCreated(this);
            OnAwake(new ScriptArgumentsCollection(args));

            m_canUpdate = true;
        }
        internal void Update()
        {
            if (!m_canUpdate)
                return;

            OnUpdate();
        }
        internal void PreDestroy()
        {
            if (IsDestroyed)
                return;

            IsDestroyed = true;

            OnDestroy();
            RealtimeManager.OnScriptPreDestroyed(this);
        }

        /// <summary>
        /// Occurs when the script initializes
        /// </summary>
        /// <param name="args">The arguments passed to the script initialization</param>
        protected virtual void OnAwake(ScriptArgumentsCollection args) { }
        /// <summary>
        /// Occurs when the script is updated (each frame)
        /// </summary>
        protected abstract void OnUpdate();
        /// <summary>
        /// Occurs before the script is destroyed
        /// </summary>
        protected virtual void OnDestroy() { }

        /// <summary>
        /// Destroy the current instance of the script
        /// </summary>
        public void Destroy()
        {
            Dispose();
        }
        /// <summary>
        /// Dispose the current instance
        /// </summary>
        public void Dispose()
        {
            if (IsDestroyed || IsDisposed)
                return;

            IsDisposed = true;
            RealtimeManager.OnScriptDestroyed(this);
        }
    }
}
