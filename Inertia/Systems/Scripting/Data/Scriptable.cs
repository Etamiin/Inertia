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

        internal int Id;
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
            Id = ScriptingManager.Current.GenerateScriptId();
            UpdateMethod = GetType().GetMethod("Update");
            OnDestroyedMethod = GetType().GetMethod("OnDestroyed");

            InternalAwake();
        }
        
        internal void InternalAwake()
        {
            if (IsAwaked)
                return;

            var method = GetType().GetMethod("Awake");
            if (method != null)
                try { method.Invoke(this, new object[] { }); } catch (Exception e) { Logger.Error(e); };

            IsAwaked = true;
        }
        internal void InternalStart()
        {
            if (!IsAwaked || IsStarted)
                return;

            var method = GetType().GetMethod("Start");
            if (method != null)
                try { method.Invoke(this, new object[] { }); } catch (Exception e) { Logger.Error(e); };

            ScriptingManager.ScriptStarted(this);
            IsStarted = true;
        }
        internal void InternalUpdate()
        {
            if (IsDestroyed)
                return;

            if (IsStarted)
                try { UpdateMethod?.Invoke(this, new object[] { }); } catch (Exception e) { Logger.Error(e); };
            FirstUpdateExecuted = true;
        }

        public virtual void Awake() { }
        public virtual void Start() { }
        public virtual void Update() { }
        public virtual void OnDestroyed() { }

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
            try { OnDestroyedMethod?.Invoke(this, new object[] { }); } catch (Exception e) { Logger.Error(e); };

            Collection.Remove(Id);
            Collection = null;
            IsDestroyed = true;
        }
    }
}
