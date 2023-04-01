using System;

namespace Inertia.Scriptable
{
    public class TimedPaper : PaperObject
    {
        public static void OnNextTick(BasicAction action)
        {
            PaperFactory.CreateAndActive<TimedPaper>(action);
        }
        public static void Delayed(float delayTimeSeconds, BasicAction action)
        {
            PaperFactory.CreateAndActive<TimedPaper>(action, delayTimeSeconds, false);
        }
        public static void Delayed(TimeSpan delayTime, BasicAction action)
        {
            PaperFactory.CreateAndActive<TimedPaper>(action, delayTime, false);
        }
        public static TimedPaper DelayedLoop(float delayTimeSeconds, BasicAction action)
        {
            return PaperFactory.CreateAndActive<TimedPaper>(action, delayTimeSeconds, true);
        }
        public static TimedPaper DelayedLoop(TimeSpan delayTime, BasicAction action)
        {
            return PaperFactory.CreateAndActive<TimedPaper>(action, delayTime, true);
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

        private readonly BasicAction? _action;
        private readonly bool _disposeWhenExecuted;
        private readonly TimeSpan _delayTime;
        private DateTime _startAt;

        internal TimedPaper(BasicAction action) : this(action, TimeSpan.Zero)
        {
        }
        internal TimedPaper(BasicAction action, float delayTimeSeconds, bool loopExecution = false) : this(action, TimeSpan.FromSeconds(delayTimeSeconds), loopExecution)
        {
        }
        internal TimedPaper(BasicAction action, TimeSpan delayTime, bool loopExecution = false)
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
            try
            {
                _action?.Invoke();
            }
            finally
            {
                if (!_disposeWhenExecuted)
                {
                    _startAt = DateTime.Now;
                }
                else Dispose();
            }
        }
    }
}
