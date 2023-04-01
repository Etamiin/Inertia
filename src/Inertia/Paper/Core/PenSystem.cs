using System;
using System.Collections.Generic;
using System.Linq;

namespace Inertia.Scriptable
{
    public abstract class PenSystem<T> : IPenSystem where T : PaperObject
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

        private readonly PenExecutionLayer _executionLayer;
        private readonly List<T> _componentDatas;
        private readonly object _locker;
        private bool _processingRegistered;

        protected PenSystem()
        {
            _locker = new object();
            _componentDatas = new List<T>();
            _executionLayer = PaperFactory.RegisterScriptableSystem(this);
        }

        public abstract void OnProcess(T obj);
        public abstract void OnExceptionThrown(Exception exception);

        void IPenSystem.RegisterComponentData(PaperObject obj)
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
        void IPenSystem.UnregisterComponentData(PaperObject obj)
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
                var executableDatas = _componentDatas.Where((obj) => obj.State == PaperObject.PaperObjectState.Initialized).ToArray();

                try
                {
                    foreach (var obj in executableDatas)
                    {
                        if (obj.State == PaperObject.PaperObjectState.Disposed) continue;

                        OnProcess(obj);
                    }
                }
                catch (Exception ex)
                {
                    OnExceptionThrown(ex);
                }

                var notInitializedDatas = _componentDatas.Where((obj) => obj.State != PaperObject.PaperObjectState.Initialized).ToArray();
                foreach (var obj in notInitializedDatas)
                {
                    if (obj.State == PaperObject.PaperObjectState.Disposing)
                    {
                        obj.State = PaperObject.PaperObjectState.Disposed;
                        ((IPenSystem)this).UnregisterComponentData(obj);
                    }
                }
            }
        }
    }
}
