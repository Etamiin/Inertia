using System;

namespace Inertia.Paper
{
    public class TimedPaper : PaperObject
    {
        public static void OnNextTick(Action action)
        {
            TimedPenSystem.BeginPaper(action);
        }
        public static void Delayed(float delayTimeSeconds, Action action)
        {
            TimedPenSystem.BeginPaper(action, delayTimeSeconds, false);
        }
        public static void Delayed(TimeSpan delayTime, Action action)
        {
            TimedPenSystem.BeginPaper(action, delayTime, false);
        }
        public static TimedPaper DelayedLoop(float delayTimeSeconds, Action action)
        {
            return TimedPenSystem.BeginPaper(action, delayTimeSeconds, true);
        }
        public static TimedPaper DelayedLoop(TimeSpan delayTime, Action action)
        {
            return TimedPenSystem.BeginPaper(action, delayTime, true);
        }

        internal bool CanBeExecuted
        {
            get
            {
                if (IsDisposed) return false;
                if (_delayTime == TimeSpan.Zero) return true;

                var elapsed = DateTime.Now - _startAt;
                return elapsed >= _delayTime;
            }
        }

        private readonly Action? _action;
        private readonly bool _disposeWhenExecuted;
        private readonly TimeSpan _delayTime;
        private DateTime _startAt;

        internal TimedPaper(Action action) : this(action, TimeSpan.Zero)
        {
        }
        internal TimedPaper(Action action, float delayTimeSeconds, bool loopExecution = false) : this(action, TimeSpan.FromSeconds(delayTimeSeconds), loopExecution)
        {
        }
        internal TimedPaper(Action action, TimeSpan delayTime, bool loopExecution = false)
        {
            _disposeWhenExecuted = !loopExecution;
            _action = action;
            _delayTime = delayTime;
            _startAt = DateTime.Now;

            if (_action == null)
            {
                _disposeWhenExecuted = true;
            }
        }

        internal void Execute()
        {
            _action?.Invoke();

            if (!_disposeWhenExecuted)
            {
                _startAt = DateTime.Now;
            }
            else Dispose();
        }
    }
}