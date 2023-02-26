using Inertia.Runtime.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Inertia.Runtime
{
    internal static class RuntimeManager
    {
        private static Dictionary<int, ScriptExecutionLayer> _executionLayers;
        private static Dictionary<Type, IScriptComponent> _componentInstances;

        static RuntimeManager()
        {
            _componentInstances = new Dictionary<Type, IScriptComponent>();
            _executionLayers = new Dictionary<int, ScriptExecutionLayer>();
        }

        internal static void RegisterComponentData(ScriptComponentData componentData)
        {
            if (ReflectionProvider.TryGetScriptComponent(componentData.GetType(), out var componentType))
            {
                if (!_componentInstances.TryGetValue(componentType, out var componentInstance))
                {
                    componentInstance = (IScriptComponent)Activator.CreateInstance(componentType);
                    if (!_executionLayers.TryGetValue(componentInstance.ExecutionLayer, out var executionLayer))
                    {
                        executionLayer = new ScriptExecutionLayer();
                        _executionLayers.Add(componentInstance.ExecutionLayer, executionLayer);
                    }

                    executionLayer.RegisterScriptComponent(componentInstance);
                }

                componentInstance.RegisterComponentData(componentData);
                componentData.Destroyed += () =>
                {
                    componentInstance.UnregisterComponentData(componentData);
                };
            }
        }
    }
}