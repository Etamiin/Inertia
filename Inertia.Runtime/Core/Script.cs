using System;

namespace Inertia.Runtime
{
    /// <summary>
    ///
    /// </summary>
    public abstract class Script : IDisposable
    {
        internal const int FixedMaxExecutionPerSecond = 160;

        /// <summary>
        /// Returns the time elapsed since the last execution frame. 
        /// </summary>
        public static float DeltaTime { get; internal set; }
        /// <summary>
        /// Returns the time elapsed since the Runtime loop was launched.
        /// </summary>
        public static float Time { get; internal set; }

        /// <summary>
        /// Set or Get the maximum number of executions per second (max: <see cref="FixedMaxExecutionPerSecond"/>).
        /// </summary>
        public static int MaxExecutionPerSecond = FixedMaxExecutionPerSecond;

        /// <summary>
        /// Returns true is the current instance is destroyed.
        /// </summary>
        public bool IsDestroyed { get; private set; }

        internal ScriptExecutorLayer AttachedLayer;
        internal ScriptCollection InCollection;
        internal bool IsDisposed { get; private set; }

        private bool m_canUpdate;

        internal void Awake(object[] args)
        {
            RuntimeManager.OnScriptCreated(this);
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
            RuntimeManager.OnScriptPreDestroyed(this);
        }

        /// <summary>
        /// Occurs when the script initializes.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnAwake(ScriptArgumentsCollection args) { }
        /// <summary>
        /// Occurs each execution frame.
        /// </summary>
        protected virtual void OnUpdate() { }
        /// <summary>
        /// Occurs before the script is destroyed.
        /// </summary>
        protected virtual void OnDestroy() { }

        /// <summary>
        /// 
        /// </summary>
        public void Destroy()
        {
            Dispose();
        }
        /// <summary>
        ///
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (IsDestroyed || IsDisposed)
                return;

            RuntimeManager.OnScriptDestroyed(this);
            IsDisposed = true;
        }
    }
}
