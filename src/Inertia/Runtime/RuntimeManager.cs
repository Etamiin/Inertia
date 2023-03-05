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

            ReflectionProvider.Invalidate();
        }

        internal static IScriptComponent GetScriptComponent(Type dataType)
        {
            if (_componentInstances.TryGetValue(dataType, out var component)) return component;
            
            return default;
        }
        internal static void RegisterScriptComponent<T>(ScriptComponent<T> component) where T : ScriptComponentData
        {
            if (component.IsRegistered) return;

            var dataType = typeof(T);
            if (_componentInstances.ContainsKey(dataType))
            {
                _componentInstances[dataType] = component;
            }
            else _componentInstances.Add(dataType, component);

            if (!_executionLayers.TryGetValue(component.ExecutionLayer, out var executionLayer))
            {
                executionLayer = new ScriptExecutionLayer();
                _executionLayers.Add(component.ExecutionLayer, executionLayer);
            }

            executionLayer.ComponentsUpdate += component.ProcessComponents;
            component.IsRegistered = true;
        }
    }
}