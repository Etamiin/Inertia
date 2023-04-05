using System;
using System.Collections.Generic;
using System.Text;

namespace Inertia
{
    public sealed class SyncActionQueue : ActionQueue
    {
        public SyncActionQueue(ActionQueueParameters parameters) : base(parameters)
        {
        }

        public void Execute()
        {
            if (DisposeRequested) return;

            ProcessQueue();
        }

        public override void RequestDispose()
        {
            base.RequestDispose();
            Clean();
        }
    }
}
