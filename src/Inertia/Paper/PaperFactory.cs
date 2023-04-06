using Inertia.Plugins;
using System;
using System.Collections.Concurrent;

namespace Inertia.Paper
{
    public static class PaperFactory
    {
        private static ConcurrentDictionary<int, PenExecutionLayer> _executionLayers;
        private static ConcurrentDictionary<Type, IPenSystem> _componentInstances;

        static PaperFactory()
        {
            _componentInstances = new ConcurrentDictionary<Type, IPenSystem>();
            _executionLayers = new ConcurrentDictionary<int, PenExecutionLayer>();

            ReflectionProvider.Invalidate();
        }

        public static void ConfigureLayer(int layerIndex, PenExecutionLayerType type)
        {
            ConfigureLayer(layerIndex, -1, type);
        }
        public static void ConfigureLayer(int layerIndex, int tickPerSecond, PenExecutionLayerType type)
        {
            if (!_executionLayers.TryGetValue(layerIndex, out var layer))
            {
                layer = new PenExecutionLayer(tickPerSecond, type);
                _executionLayers.TryAdd(layerIndex, layer);
            }
            else
            {
                layer.Change(tickPerSecond, type);
            }
        }
        public static void CallCycle(float deltaTime)
        {
            if (!ReflectionProvider.IsPaperOwned) return;

            var layers = _executionLayers.Values;
            foreach (var layer in layers)
            {
                layer.OnComponentsUpdate(deltaTime);
            }
        }
        public static PluginExecutionResult TryStartPlugin(string pluginFilePath, params object[] executionParameters)
        {
            return ReflectionProvider.TryStartPlugin(pluginFilePath, executionParameters);
        }
        public static bool TryStopPlugin(string pluginIdentifier)
        {
            return ReflectionProvider.TryStopPlugin(pluginIdentifier);
        }

        internal static IPenSystem GetScriptableSystem(Type dataType)
        {
            _componentInstances.TryGetValue(dataType, out var component);
            return component;
        }
        internal static PenExecutionLayer RegisterScriptableSystem<T>(PenSystem<T> component) where T : PaperObject
        {
            var dataType = typeof(T);
            if (_componentInstances.ContainsKey(dataType))
            {
                throw new PenSystemDuplicateException(component.GetType(), dataType);
            }
            else _componentInstances.TryAdd(dataType, component);

            if (!_executionLayers.TryGetValue(component.LayerIndex, out var executionLayer))
            {
                ConfigureLayer(component.LayerIndex, PenExecutionLayerType.FixedSleep);
                executionLayer = _executionLayers[component.LayerIndex];
            }

            return executionLayer;
        }
    }
}