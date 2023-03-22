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

        private readonly ScriptableExecutionLayer _executionLayer;
        private readonly List<T> _componentDatas;
        private readonly object _locker;
        private bool _processingRegistered;

        public ScriptableSystem()
        {
            _locker = new object();
            _componentDatas = new List<T>();
            _executionLayer = RuntimeManager.RegisterScriptComponent(this);
        }

        public abstract void OnProcess(IEnumerable<T> componentDatas);
        
        void IScriptable.RegisterComponentData(ScriptableData componentData)
        {
            if (componentData is T tData)
            {
                lock (_locker)
                {
                    _componentDatas.Add(tData);

                    if (!_processingRegistered)
                    {
                        _executionLayer.ComponentsUpdate += ProcessComponents;
                        _processingRegistered = true;
                    }
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

                    if (_processingRegistered && !IsActive)
                    {
                        _executionLayer.ComponentsUpdate -= ProcessComponents;
                        _processingRegistered = false;
                    }
                }
                
                componentData.Destroying -= ComponentData_Destroying;
            }
        }

        private void ProcessComponents(float deltaTime)
        {
            DeltaTime = deltaTime;

            if (IsActive)
            {
                lock (_locker)
                {
                    var executableDatas = _componentDatas.Where((data) => data.State == ScriptableData.ScriptableDataState.Initialized);
                    OnProcess(executableDatas);

                    var notInitializedDatas = _componentDatas.Where((data) => data.State != ScriptableData.ScriptableDataState.Initialized).ToArray();
                    foreach (var data in notInitializedDatas)
                    {
                        if (data.State == ScriptableData.ScriptableDataState.Disposing)
                        {
                            data.Destroy();
                        }
                        else if (data.State == ScriptableData.ScriptableDataState.Initializing)
                        {
                            data.State = ScriptableData.ScriptableDataState.Initialized;
                        }
                    }
                }
            }
        }
        private void ComponentData_Destroying(ScriptableData componentData)
        {
            ((IScriptable)this).UnregisterComponentData(componentData);
        }
    }
}
