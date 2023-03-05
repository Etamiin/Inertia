using Inertia.Runtime.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Inertia.Runtime.Core
{
    public abstract class ScriptComponent<T> : IScriptComponent where T : ScriptComponentData
    {
        public float DeltaTime { get; private set; }
        public virtual int ExecutionLayer { get; }
        public bool IsActive { get; private set; }

        internal bool IsRegistered { get; set; }

        private object _locker;
        private List<T> _componentDatas;
        
        public ScriptComponent()
        {
            _locker = new object();
            _componentDatas = new List<T>();

            RuntimeManager.RegisterScriptComponent(this);
        }

        public abstract void OnUpdate(T componentData);

        internal void ProcessComponents(float deltaTime)
        {
            if (!IsRegistered) return;

            DeltaTime = deltaTime;

            if (IsActive)
            {
                lock (_locker)
                {
                    foreach (var componentData in _componentDatas)
                    {
                        OnUpdate(componentData);
                    }
                }
            }
        }

        public void RegisterComponentData(ScriptComponentData componentData)
        {
            if (componentData is T tData)
            {
                lock (_locker)
                {
                    _componentDatas.Add(tData);
                    IsActive = _componentDatas.Count > 0;

                    componentData.Destroying += () => { 
                        UnregisterComponentData(componentData);
                    };
                }
            }
        }
        public void UnregisterComponentData(ScriptComponentData componentData)
        {
            if (componentData is T tData)
            {
                lock (_locker)
                {
                    _componentDatas.Remove(tData);
                    IsActive = _componentDatas.Count > 0;
                }
            }
        }
    }
}
