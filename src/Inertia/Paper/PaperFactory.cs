using System;
using System.Collections.Concurrent;

namespace Inertia.Paper
{
    public static class PaperFactory
    {
        internal static event Action<float> LayersTick;

        private static ConcurrentDictionary<Type, IPenSystem> _componentInstances;
        private static ConcurrentDictionary<int, PenExecutionLayer> _executionLayers;

        static PaperFactory()
        {
            _componentInstances = new ConcurrentDictionary<Type, IPenSystem>();
            _executionLayers = new ConcurrentDictionary<int, PenExecutionLayer>();

            ReflectionProvider.Invalidate();
        }

        public static void ConfigureLayer(int layerIndex)
        {
            ConfigureLayer(layerIndex, -1);
        }
        public static void ConfigureLayer(int layerIndex, int tickPerSecond)
        {
            if (!_executionLayers.TryGetValue(layerIndex, out var layer))
            {
                layer = new PenExecutionLayer(tickPerSecond);
                _executionLayers.TryAdd(layerIndex, layer);
            }
            else
            {
                layer.Change(tickPerSecond);
            }
        }
        public static void CallCycle(float deltaTime)
        {
            if (!ReflectionProvider.IsPaperOwned) throw new InvalidOperationException($"{nameof(CallCycle)} can only be called when {nameof(PaperOwnerAttribute)} attribute is defined.");

            LayersTick?.Invoke(deltaTime);
        }

        internal static IPenSystem GetPenSystem(Type paperObjectType)
        {
            do
            {
                if (_componentInstances.TryGetValue(paperObjectType, out var component)) return component;

                paperObjectType = paperObjectType.BaseType;
            }
            while (paperObjectType.BaseType != null);

            throw new InvalidCastException($"Type '{paperObjectType.Name}' is invalid, no PenSystem is attached to this type.");
        }
        internal static PenExecutionLayer RegisterPenSystem<T>(PenSystem<T> component) where T : PaperObject
        {
            var paperObjectType = typeof(T);
            if (_componentInstances.ContainsKey(paperObjectType))
            {
                throw new PenSystemDuplicateException(component.GetType(), paperObjectType);
            }
            else _componentInstances.TryAdd(paperObjectType, component);

            if (!_executionLayers.TryGetValue(component.LayerIndex, out var executionLayer))
            {
                ConfigureLayer(component.LayerIndex);
                executionLayer = _executionLayers[component.LayerIndex];
            }

            return executionLayer;
        }
    }
}