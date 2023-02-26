using Inertia.Runtime.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Inertia.Runtime.Core
{
    public abstract class ScriptComponent<T> : IScriptComponent where T : ScriptComponentData
    {
        public event BasicAction? Destroyed;

        private object _locker;
        private List<T> _componentDatas;

        public float DeltaTime { get; private set; }
        public virtual int ExecutionLayer { get; }

        public ScriptComponent()
        {
            _locker = new object();
            _componentDatas = new List<T>();
        }

        public abstract void OnUpdate(T componentData);

        internal void ProcessComponents()
        {
            lock (_locker)
            {
                foreach (var componentData in _componentDatas)
                {
                    OnUpdate(componentData);
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
                }
            }
        }

        void IScriptComponent.ProcessComponents(float deltaTime)
        {
            DeltaTime = deltaTime;
            lock (_locker)
            {
                foreach (var componentData in _componentDatas)
                {
                    OnUpdate(componentData);
                }
            }
        }
    }
}
