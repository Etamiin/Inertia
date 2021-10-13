using System;

namespace Inertia.Runtime
{
    /// <summary>
    ///
    /// </summary>
    public abstract class Script : IDisposable
    {
        /// <summary>
        /// Returns the time elapsed since the last execution frame. 
        /// </summary>
        public static float DeltaTime { get; internal set; }

        /// <summary>
        /// Returns true is the current instance is destroyed.
        /// </summary>
        public bool IsDestroyed { get; private set; }

        internal ScriptCollection Parent;
        internal bool IsDisposed { get; private set; }

        private bool _canUpdate;

        internal void Awake(object[] args)
        {
            RuntimeManager.OnScriptCreated(this);
            OnAwake(new ScriptArgumentsCollection(args));

            _canUpdate = true;
        }
        internal void Update()
        {
            if (_canUpdate)
            {
                OnUpdate();
            }
        }
        internal void PreDestroy()
        {
            if (!IsDestroyed)
            {
                IsDestroyed = true;

                OnDestroy();
                RuntimeManager.OnScriptPreDestroyed(this);
            }
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
            Dispose(true);
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
            if (!IsDestroyed && !IsDisposed)
            {
                RuntimeManager.OnScriptDestroyed(this);
                IsDisposed = true;
            }
        }
    }
}
