﻿using Inertia.Logging;
using System.Collections.Generic;

namespace Inertia.Scriptable
{
    internal class TimedScriptableSystem : ScriptableSystem<TimedScriptData>
    {
        public override void OnProcess(IEnumerable<TimedScriptData> componentDatas)
        {
            foreach (var script in componentDatas)
            {
                if (script.CanBeExecuted) script.Execute();
            }
        }
    }
}