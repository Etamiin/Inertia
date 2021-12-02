using System;

namespace Inertia.Runtime
{
    public abstract class Script : IDisposable
    {
        /// <summary>
        /// Returns the time elapsed since the last execution frame. 
        /// </summary>
        public static float DeltaTime { get; internal set; }

        public bool IsDestroyed { get; private set; }

        internal bool IsDisposed { get; private set; }

        public void Destroy()
        {
            Dispose();
        }
        public void Dispose()
        {
            if (!IsDestroyed)
            {
                RuntimeManager.BeginUnregisterScript(this);
                IsDisposed = true;
            }
        }

        /// <summary>
        /// Occurs each execution frame.
        /// </summary>
        internal protected abstract void OnUpdate();

        internal void PreDestroy()
        {
            if (!IsDestroyed)
            {
                IsDestroyed = true;

                OnDestroy();
                RuntimeManager.EndUnregisterScript(this);
            }
        }

        /// <summary>
        /// Occurs when the script initializes.
        /// </summary>
        /// <param name="args"></param>
        internal protected virtual void OnInitialize(ScriptArguments args) { }
        /// <summary>
        /// Occurs before the script is destroyed.
        /// </summary>
        internal protected virtual void OnDestroy() { }
    }
}