using Inertia.Logging;
using System;
using System.Collections.Generic;

namespace Inertia.Scriptable
{
    internal class TimedScriptableSystem : ScriptableSystem<TimedScriptData>
    {
        public override bool ProcessIndividualTryCatch => false;

        public override void OnProcess(TimedScriptData script)
        {
            if (script.CanBeExecuted) script.Execute();
        }

        public override void OnExceptionThrown(Exception exception) { }
    }
}
