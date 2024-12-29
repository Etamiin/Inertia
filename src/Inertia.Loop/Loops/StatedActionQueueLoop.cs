using System;
using System.Collections.Generic;

namespace Inertia.Loop
{
    public class StatedActionQueueLoop : LoopBase
    {
        private List<StatedAction> _statedActions;
        private object _lock;

        public StatedActionQueueLoop() : this(LoopMode.AlwaysSleep, new LoopTime())
        {
        }
        public StatedActionQueueLoop(LoopMode loopMode) : this(loopMode, new LoopTime())
        {
        }
        public StatedActionQueueLoop(LoopTime loopTime) : this(LoopMode.AlwaysSleep, loopTime)
        {
        }
        public StatedActionQueueLoop(LoopMode loopMode, LoopTime loopTime) : base(loopMode, loopTime)
        {
            _statedActions = new List<StatedAction>();
            _lock = new object();
        }

        public void Enqueue(Action action)
        {
            DoEnqueue(new StatedAction(action));
        }
        public void Enqueue(float delay, Action action)
        {
            Enqueue(delay, null, action);
        }
        public void Enqueue(Func<bool>? condition, Action action)
        {
            Enqueue(0, condition, action);
        }
        public void Enqueue(float delay, Func<bool>? condition, Action action)
        {
            DoEnqueue(new StatedAction(action)
            {
                Delay = delay,
                Condition = condition
            });
        }
        public void Enqueue(StatedAction action)
        {
            DoEnqueue(action);
        }

        protected override void DoRun(LoopTime loopTime)
        {
            this.ThrowIfDisposable(IsDisposed);

            if (_statedActions == null) return;

            lock (_lock)
            {
                var actions = _statedActions.ToArray();

                foreach (var statedAction in actions)
                {
                    statedAction.TryInvoke(loopTime);
                }

                _statedActions.RemoveAll((sa) => sa.Invoked);
            }
        }
        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed && disposing)
            {
                lock (_lock)
                {
                    _statedActions.Clear();
                }
            }

            base.Dispose(disposing);
        }
    
        private void DoEnqueue(StatedAction action)
        {
            this.ThrowIfDisposable(IsDisposed);

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            lock (_lock)
            {
                action.StartedAt = LoopTime.Time;

                _statedActions.Add(action);
            }
        }
    }
}