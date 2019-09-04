using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Inertia
{
    public abstract class Scriptable
    {
        internal uint Id;
        internal bool DestroyRequested = false;
        internal ObjectScriptable Parent;

        public bool IsDestroyed { get; private set; }

        private bool _deletable => (_isStarted && _firstUpdate);
        private bool _isAwaked;
        private bool _isStarted;
        private bool _firstUpdate;
        private MethodInfo _updateMethod;
        private MethodInfo _onDestroyedMethod;

        internal void SetParameters(ObjectScriptable parent, uint Id)
        {
            if (_isAwaked)
                return;

            this.Id = Id;
            this.Parent = parent;

            _updateMethod = GetType().GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance);
            _onDestroyedMethod = GetType().GetMethod("OnDestroyed", BindingFlags.NonPublic | BindingFlags.Instance);

            ScriptingModule.GenerateScriptEvents(this);
            InternalAwake();
        }

        internal void InternalAwake()
        {
            if (_isAwaked)
                return;

            var method = GetType().GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method != null)
                try { method.Invoke(this, new object[] { }); } catch (Exception e) { Logger.Error(e); };

            _isAwaked = true;
        }
        internal void InternalStart()
        {
            if (!_isAwaked || _isStarted)
                return;

            var method = GetType().GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method != null)
                try { method.Invoke(this, new object[] { }); } catch (Exception e) { Logger.Error(e); };

            ScriptingModule.ScriptStarted(this);
            _isStarted = true;
        }
        internal void InternalUpdate()
        {
            if (IsDestroyed)
                return;

            if (_isStarted)
                try { _updateMethod?.Invoke(this, new object[] { }); } catch (Exception e) { Logger.Error(e); };
            _firstUpdate = true;
        }

        public void Dispose()
        {
            if (IsDestroyed)
                return;

            ScriptingModule.DeleteScript(this);
        }
        internal void DestroyCheck()
        {
            if (!_deletable)
                return;

            ScriptingModule.OnScriptDeleted(this);
            try { _onDestroyedMethod?.Invoke(this, new object[] { }); } catch (Exception e) { Logger.Error(e); };

            Parent = null;
            IsDestroyed = true;
        }
    }
}
