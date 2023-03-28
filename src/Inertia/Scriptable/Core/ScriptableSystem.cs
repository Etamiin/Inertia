using System;
using System.Collections.Generic;
using System.Linq;

namespace Inertia.Scriptable
{
    public abstract class ScriptableSystem<T> : IScriptableSystem where T : ScriptableObject
    {
        public abstract bool ProcessIndividualTryCatch { get; }

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

        protected ScriptableSystem()
        {
            _locker = new object();
            _componentDatas = new List<T>();
            _executionLayer = RuntimeManager.RegisterScriptComponent(this);
        }

        public abstract void OnProcess(T obj);
        public abstract void OnExceptionThrown(Exception exception);

        void IScriptableSystem.RegisterComponentData(ScriptableObject obj)
        {
            if (obj is T tData)
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
            }
        }
        void IScriptableSystem.UnregisterComponentData(ScriptableObject obj)
        {
            if (obj is T tData)
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
            }
        }

        private void ProcessComponents(float deltaTime)
        {
            DeltaTime = deltaTime;

            if (!IsActive) return;
            lock (_locker)
            {
                var executableDatas = _componentDatas.Where((obj) => obj.State == ScriptableObject.ScriptableDataState.Initialized).ToArray();

                if (ProcessIndividualTryCatch)
                {
                    foreach (var obj in executableDatas)
                    {
                        if (obj.State == ScriptableObject.ScriptableDataState.Disposed) continue;

                        try
                        {
                            OnProcess(obj);
                        }
                        catch (Exception ex)
                        {
                            OnExceptionThrown(ex);
                        }
                    }
                }
                else
                {
                    try
                    {
                        foreach (var obj in executableDatas)
                        {
                            OnProcess(obj);
                        }
                    }
                    catch (Exception ex)
                    {
                        OnExceptionThrown(ex);
                    }
                }

                var notInitializedDatas = _componentDatas.Where((obj) => obj.State != ScriptableObject.ScriptableDataState.Initialized).ToArray();
                foreach (var obj in notInitializedDatas)
                {
                    if (obj.State == ScriptableObject.ScriptableDataState.Disposing)
                    {
                        obj.State = ScriptableObject.ScriptableDataState.Disposed;
                        ((IScriptableSystem)this).UnregisterComponentData(obj);
                    }
                }
            }
        }
    }
}
