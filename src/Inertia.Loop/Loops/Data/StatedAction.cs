using System;

namespace Inertia.Loop
{
    public class StatedAction
    {
        private Action _action;
        
        public StatedAction(Action action)
        {
            _action = action;
        }

        internal float StartedAt { get; set; }
        internal bool Invoked { get; private set; }

        public float Delay { get; set; }
        public Func<bool>? Condition { get; set; }

        internal void TryInvoke(LoopTime time)
        {
            if (Condition != null && !Condition()) return;

            if (Delay > 0)
            {
                var elapsed = time.Time - StartedAt;
                if (elapsed < Delay) return;
            }

            _action();
            Invoked = true;
        }
    }
}
