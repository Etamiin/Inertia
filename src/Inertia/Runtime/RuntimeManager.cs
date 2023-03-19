using System;
using System.Collections.Generic;

namespace Inertia.Scriptable
{
    internal static class RuntimeManager
    {
        internal static TimeSpan ScriptableDataSleepTime = TimeSpan.FromMilliseconds(50);
        internal static AsyncExecutionQueuePool QueuePool { get; private set; }

        private static Dictionary<int, ScriptableExecutionLayer> _executionLayers;
        private static Dictionary<Type, IScriptable> _componentInstances;

        static RuntimeManager()
        {
            QueuePool = new AsyncExecutionQueuePool(500);
            _componentInstances = new Dictionary<Type, IScriptable>();
            _executionLayers = new Dictionary<int, ScriptableExecutionLayer>();

            ReflectionProvider.Invalidate();
        }

        internal static IScriptable GetScriptableSystem(Type dataType)
        {
            if (_componentInstances.TryGetValue(dataType, out var component)) return component;
            
            return default;
        }
        internal static void RegisterScriptComponent<T>(ScriptableSystem<T> component) where T : ScriptableData
        {
            if (component.IsRegistered) return;

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

            executionLayer.ComponentsUpdate += component.ProcessComponents;
            component.IsRegistered = true;
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