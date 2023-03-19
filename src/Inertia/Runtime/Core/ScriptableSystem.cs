using System;
using System.Collections.Generic;
using System.Linq;

namespace Inertia.Scriptable
{
    public abstract class ScriptableSystem<T> : IScriptable where T : ScriptableData
    {
        public virtual int ExecutionLayer { get; }
        public float DeltaTime { get; private set; }
        public bool IsActive
        {
            get
            {
                return _componentDatas.Count > 0;
            }
        }

        internal bool IsRegistered { get; set; }

        private object _locker;
        private List<T> _componentDatas;
        
        public ScriptableSystem()
        {
            _locker = new object();
            _componentDatas = new List<T>();

            RuntimeManager.RegisterScriptComponent(this);
        }

        public abstract void OnProcess(IEnumerable<T> componentDatas);
        
        internal void ProcessComponents(float deltaTime)
        {
            if (!IsRegistered) return;

            DeltaTime = deltaTime;

            if (IsActive)
            {
                lock (_locker)
                {
                    var executableDatas = _componentDatas.Where((data) => data.CanBeProcessed).ToArray();
                    OnProcess(executableDatas);

                    var destroyableDatas = _componentDatas.Where((data) => data.DisposeRequested).ToArray();
                    foreach (var data in destroyableDatas)
                    {
                        data.Destroy();
                    }
                }
            }
        }

        void IScriptable.RegisterComponentData(ScriptableData componentData)
        {
            if (componentData is T tData)
            {
                lock (_locker)
                {
                    _componentDatas.Add(tData);
                }

                componentData.Destroying += ComponentData_Destroying;
            }
        }
        void IScriptable.UnregisterComponentData(ScriptableData componentData)
        {
            if (componentData is T tData)
            {
                lock (_locker)
                {
                    _componentDatas.Remove(tData);
                }
                
                componentData.Destroying -= ComponentData_Destroying;
            }
        }

        private void ComponentData_Destroying(ScriptableData componentData)
        {
            ((IScriptable)this).UnregisterComponentData(componentData);
        }
    }
}
