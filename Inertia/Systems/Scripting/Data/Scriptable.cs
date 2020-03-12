using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Inertia.Internal;

namespace Inertia.Scripting
{
    public abstract class Scriptable
    {
        #region Internal variables

        internal bool DestroyRequested = false;
        internal ScriptCollection Collection;
        internal Updater Updater;

        #endregion

        #region Public variables

        public bool IsDestroyed { get; private set; }

        #endregion
        
        #region Private variables

        private bool Deletable => IsStarted && FirstUpdateExecuted;
        private bool IsAwaked;
        private bool IsStarted;
        private bool FirstUpdateExecuted;
        private MethodInfo UpdateMethod;
        private MethodInfo OnDestroyedMethod;

        #endregion

        internal void SetParameters(ScriptCollection collection)
        {
            if (IsAwaked)
                return;

            ScriptingManager.GenerateScriptEvents(this);

            Collection = collection;
            UpdateMethod = GetType().GetMethod("Update");
            OnDestroyedMethod = GetType().GetMethod("OnDestroyed");

            InternalAwake();
        }
        
        internal void InternalAwake()
        {
            if (IsAwaked)
                return;

            Awake();
            IsAwaked = true;
        }
        internal void InternalStart()
        {
            if (!IsAwaked || IsStarted)
                return;

            Start();
            ScriptingManager.ScriptStarted(this);
            IsStarted = true;
        }
        internal void InternalUpdate()
        {
            if (IsDestroyed)
                return;

            Update();
            FirstUpdateExecuted = true;
        }

        protected virtual void Awake() { }
        protected virtual void Start() { }
        protected virtual void Update() { }
        protected virtual void OnDestroyed() { }

        public void Destroy()
        {
            Dispose();
        }
        public void Dispose()
        {
            if (IsDestroyed)
                return;

            ScriptingManager.DeleteScript(this);
        }
        internal void DestroyCheck()
        {
            if (!Deletable)
                return;

            ScriptingManager.OnScriptDeleted(this);
            try { OnDestroyedMethod?.Invoke(this, new object[] { }); } catch (Exception e) { InertiaLogger.Error(e); };

            Collection.Remove(this);
            Collection = null;
            IsDestroyed = true;
        }
    }
}
