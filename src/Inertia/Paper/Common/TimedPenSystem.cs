using Inertia.Logging;
using System;

namespace Inertia.Paper
{
    internal sealed class TimedPenSystem : PenSystem<TimedPaper>
    {
        public override void OnProcess(TimedPaper script)
        {
            if (script.CanBeExecuted) script.Execute();
        }

        public override void OnExceptionThrown(PaperInstanceThrowedException<TimedPaper> paperException)
        {
            paperException.DisposeResponsibleInstance = true;

            BasicLogger.Default.Error($"'{paperException.Instance}' failed to execute: {paperException.InnerException}");
        }
    }
}
