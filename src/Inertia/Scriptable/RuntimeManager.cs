using System;
using System.Collections.Generic;

namespace Inertia.Scriptable
{
    internal static class RuntimeManager
    {
        internal static AsyncExecutionQueuePool QueuePool { get; private set; }

        private static Dictionary<int, ScriptableExecutionLayer> _executionLayers;
        private static Dictionary<Type, IScriptableSystem> _componentInstances;

        static RuntimeManager()
        {
            QueuePool = new AsyncExecutionQueuePool(500, true);
            _componentInstances = new Dictionary<Type, IScriptableSystem>();
            _executionLayers = new Dictionary<int, ScriptableExecutionLayer>();

            ReflectionProvider.Invalidate();
        }

        internal static IScriptableSystem GetScriptableSystem(Type dataType)
        {
            if (_componentInstances.TryGetValue(dataType, out var component)) return component;
            
            return default;
        }
        internal static ScriptableExecutionLayer RegisterScriptComponent<T>(ScriptableSystem<T> component) where T : ScriptableObject
        {
            var dataType = typeof(T);
            if (_componentInstances.ContainsKey(dataType))
            {
                throw new ScriptComponentSystemDuplicate(component.GetType(), dataType);
            }
            else _componentInstances.Add(dataType, component);

            if (!_executionLayers.TryGetValue(component.ExecutionLayer, out var executionLayer))
            {
                executionLayer = new ScriptableExecutionLayer();
                _executionLayers.Add(component.ExecutionLayer, executionLayer);
            }

            return executionLayer;
        }
        internal static void RuntimeCall(float deltaTime)
        {
            if (!ReflectionProvider.IsRuntimeCallOverriden) return;

            foreach (var pair in _executionLayers)
            {
                pair.Value.OnComponentsUpdate(deltaTime);
            }
        }
    }
}