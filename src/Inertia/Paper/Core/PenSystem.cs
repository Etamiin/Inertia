using Inertia.Logging;
using System.Collections.Generic;
using System.Linq;

namespace Inertia.Paper
{
    public abstract class PenSystem<T> : IPenSystem where T : PaperObject
    {
        public ILogger Logger => LoggingProvider.Logger;
        public virtual int LayerIndex { get; }
        public virtual bool RunWhenEmpty { get; }
        public float DeltaTime { get; private set; }
        public int TickCount { get; private set; }
        public bool IsActive
        {
            get
            {
                return _papers.Count > 0 || RunWhenEmpty;
            }
        }

        private readonly PenExecutionLayer _executionLayer;
        private readonly List<T> _papers;
        private readonly object _locker;
        private bool _isRunning;

        protected PenSystem()
        {
            _locker = new object();
            _papers = new List<T>();
            _executionLayer = PaperFactory.RegisterPenSystem(this);
        }

        protected abstract void Tick();
        protected abstract void Process(T obj);

        void IPenSystem.RegisterPaper(PaperObject obj)
        {
            if (obj is T tData)
            {
                lock (_locker)
                {
                    _papers.Add(tData);

                    if (!_isRunning)
                    {
                        _executionLayer.Ticking += ProcessPapers;
                        _isRunning = true;
                    }
                }
            }
        }

        private void ProcessPapers(object sender, PenLayerTickingArgs e)
        {
            DeltaTime = e.DeltaTime;
            TickCount++;

            if (!IsActive) return;

            lock (_locker)
            {
                Tick();

                var writablePapers = _papers
                    .Where((obj) => obj.State == PaperObjectState.Initialized)
                    .ToArray();

                foreach (var obj in writablePapers)
                {
                    Process(obj);
                }

                _papers.RemoveAll((o) => o.State == PaperObjectState.Disposed);

                if (_isRunning && !IsActive)
                {
                    _executionLayer.Ticking -= ProcessPapers;
                    _isRunning = false;
                }
            }
        }
    }
}