using Inertia.Logging;
using System;

namespace Inertia.Scriptable
{
    internal sealed class TimedPenSystem : PenSystem<TimedPaper>
    {
        public override void OnProcess(TimedPaper script)
        {
            if (script.CanBeExecuted) script.Execute();
        }

        public override void OnExceptionThrown(Exception exception)
        {
            SimpleLogger.Default.Error($"{nameof(TimedPaper)} failed to execute: {exception}");
        }
    }
}
