using System;

namespace Inertia.Scriptable
{
    public class TimedScriptData : ScriptableObject
    {
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

        internal TimedScriptData(BasicAction action) : this(action, TimeSpan.Zero)
        {
        }
        internal TimedScriptData(BasicAction action, float delayTimeSeconds, bool loopExecution = false) : this(action, TimeSpan.FromSeconds(delayTimeSeconds), loopExecution)
        {
        }
        internal TimedScriptData(BasicAction action, TimeSpan delayTime, bool loopExecution = false)
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
