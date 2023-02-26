using System;
using System.Collections.Generic;
using System.Text;

namespace Inertia.Runtime.Core
{
    internal interface IScriptComponent
    {
        event BasicAction? Destroyed;

        float DeltaTime { get; }
        int ExecutionLayer { get; }

        void RegisterComponentData(ScriptComponentData componentData);
        void UnregisterComponentData(ScriptComponentData componentData);

        internal void ProcessComponents(float deltaTime);
    }
}
