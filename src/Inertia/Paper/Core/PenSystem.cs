using Inertia.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Inertia.Paper
{
    public abstract class PenSystem<T> : IPenSystem where T : PaperObject
    {
        public static T CreatePaperAndActive(params object[] args)
        {
            var type = typeof(T);
            var types = args.Select((obj) => obj.GetType()).ToArray();
            var cnstr = type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, types, null);

            if (cnstr == null)
            {
                throw new NotFoundConstructorException(type, types);
            }

            var instance = (T)cnstr.Invoke(args);
            instance.SetActive();

            return instance;
        }

        public virtual int LayerIndex { get; }
        public float DeltaTime { get; private set; }
        public bool IsActive
        {
            get
            {
                return _papers.Count > 0;
            }
        }

        private readonly PenExecutionLayer _executionLayer;
        private readonly List<T> _papers;
        private readonly object _locker;
        private bool _processingRegistered;

        protected PenSystem()
        {
            _locker = new object();
            _papers = new List<T>();
            _executionLayer = PaperFactory.RegisterScriptableSystem(this);
        }

        public abstract void OnProcess(T obj);
        public abstract void OnExceptionThrown(PaperInstanceThrowedException<T> paperException);

        void IPenSystem.RegisterPaper(PaperObject obj)
        {
            if (obj is T tData)
            {
                lock (_locker)
                {
                    _papers.Add(tData);

                    if (!_processingRegistered)
                    {
                        _executionLayer.Ticking += ProcessPapers;
                        _processingRegistered = true;
                    }
                }
            }
        }
        void IPenSystem.UnregisterPaper(PaperObject obj)
        {
            if (obj is T tData)
            {
                lock (_locker)
                {
                    _papers.Remove(tData);

                    if (_processingRegistered && !IsActive)
                    {
                        _executionLayer.Ticking -= ProcessPapers;
                        _processingRegistered = false;
                    }
                }
            }
        }

        private void ProcessPapers(object sender, PenLayerTickingArgs e)
        {
            DeltaTime = e.DeltaTime;

            if (!IsActive) return;
            lock (_locker)
            {
                var writablePapers = _papers
                    .Where((obj) => obj.State == PaperObjectState.Initialized)
                    .ToArray();

                var startIndex = 0;
                ProcessPapers(writablePapers, ref startIndex);

                var notInitializedPapers = _papers
                    .Where((obj) => obj.State != PaperObjectState.Initialized)
                    .ToArray();

                foreach (var obj in notInitializedPapers)
                {
                    if (obj.State == PaperObjectState.Disposing)
                    {
                        obj.State = PaperObjectState.Disposed;
                        ((IPenSystem)this).UnregisterPaper(obj);
                    }
                }
            }
        }
        private void ProcessPapers(T[] components, ref int index)
        {
            try
            {
                for (; index < components.Length; index++)
                {
                    var obj = components[index];
                    if (obj.State != PaperObjectState.Disposed)
                    {
                        OnProcess(obj);
                    }
                }
            }
            catch (Exception ex)
            {
                var instance = components[index];
                var paperEx = new PaperInstanceThrowedException<T>(instance, ex);
                OnExceptionThrown(paperEx);

                if (paperEx.DisposeResponsibleInstance) instance.Dispose();
                if (!paperEx.StopTick && index < components.Length - 1)
                {
                    index++;
                    ProcessPapers(components, ref index);
                }
            }
        }
    }
}