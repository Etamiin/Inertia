using System;
using System.Collections.Generic;
using System.Text;

namespace Inertia.Runtime.Core
{
    public interface IScriptComponent
    {
        float DeltaTime { get; }
        int ExecutionLayer { get; }

        void RegisterComponentData(ScriptComponentData componentData);
        void UnregisterComponentData(ScriptComponentData componentData);
    }
}
