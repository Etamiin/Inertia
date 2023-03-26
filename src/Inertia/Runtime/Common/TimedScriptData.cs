using System;

namespace Inertia.Scriptable
{
    public class TimedScriptData : ScriptableData
    {
        internal BasicAction Action;
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
        internal bool DisposeWhenExecuted { get; private set; }
        
        private TimeSpan _delayTime;
        private DateTime _startAt;

        internal TimedScriptData(BasicAction action) : this(action, TimeSpan.Zero)
        {
        }
        internal TimedScriptData(BasicAction action, float delayTimeSeconds, bool loopExecution = false) : this(action, TimeSpan.FromSeconds(delayTimeSeconds), loopExecution)
        {
        }
        internal TimedScriptData(BasicAction action, TimeSpan delayTime, bool loopExecution = false)
        {
            Action = action;
            _delayTime = delayTime;
            _startAt = DateTime.Now;
            DisposeWhenExecuted = !loopExecution;

            if (Action == null) BeginDestroy();
        }

        internal void Execute()
        {
            try
            {
                Action.Invoke();
            }
            finally
            {
                if (!DisposeWhenExecuted)
                {
                    _startAt = DateTime.Now;
                }
                else BeginDestroy();
            }
        }
    }
}
