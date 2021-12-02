using System;

namespace Inertia.Runtime
{
<<<<<<< HEAD
<<<<<<< HEAD
    /// <summary>
    ///
    /// </summary>
=======
>>>>>>> 9bfc85f6784b254a10c65f104446a83c8b195c40
=======
>>>>>>> premaster
    public abstract class Script : IDisposable
    {
        /// <summary>
        /// Returns the time elapsed since the last execution frame. 
        /// </summary>
        public static float DeltaTime { get; internal set; }

<<<<<<< HEAD
<<<<<<< HEAD
        /// <summary>
        /// Returns true is the current instance is destroyed.
        /// </summary>
=======
>>>>>>> 9bfc85f6784b254a10c65f104446a83c8b195c40
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
=======
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

>>>>>>> premaster
        internal void PreDestroy()
        {
            if (!IsDestroyed)
            {
                IsDestroyed = true;

                OnDestroy();
<<<<<<< HEAD
                RuntimeManager.OnScriptPreDestroyed(this);
=======
                RuntimeManager.EndUnregisterScript(this);
>>>>>>> premaster
            }
        }

        /// <summary>
        /// Occurs when the script initializes.
        /// </summary>
        /// <param name="args"></param>
<<<<<<< HEAD
        protected virtual void OnAwake(ScriptArgumentsCollection args) { }
        /// <summary>
        /// Occurs each execution frame.
        /// </summary>
        protected virtual void OnUpdate() { }
        /// <summary>
        /// Occurs before the script is destroyed.
        /// </summary>
        protected virtual void OnDestroy() { }

<<<<<<< HEAD
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
=======
        public void Destroy()
        {
            Dispose(true);
        }        
>>>>>>> 9bfc85f6784b254a10c65f104446a83c8b195c40
        public void Dispose()
        {
            Dispose(true);
        }
<<<<<<< HEAD
        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
=======
>>>>>>> 9bfc85f6784b254a10c65f104446a83c8b195c40
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
=======
        internal protected virtual void OnInitialize(ScriptArguments args) { }
        /// <summary>
        /// Occurs before the script is destroyed.
        /// </summary>
        internal protected virtual void OnDestroy() { }
    }
}
>>>>>>> premaster
