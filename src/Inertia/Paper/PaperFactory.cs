using Inertia.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Inertia.Scriptable
{
    public static class PaperFactory
    {
        private const int MaxTickPerSecond = 180;

        internal static ActionQueuePool QueuePool { get; private set; }

        public static bool LimitProcessorUsage { get; set; }
        public static int TargetTickPerSecond
        {
            get => _targetTickPerSecond;
            set
            {
                if (value > MaxTickPerSecond)
                {
                    _targetTickPerSecond = MaxTickPerSecond;
                }
                else _targetTickPerSecond = value;
            }
        }

        private static Dictionary<int, PenExecutionLayer> _executionLayers;
        private static Dictionary<Type, IPenSystem> _componentInstances;

        private static int _targetTickPerSecond = MaxTickPerSecond;

        static PaperFactory()
        {
            QueuePool = new ActionQueuePool(500, true);
            _componentInstances = new Dictionary<Type, IPenSystem>();
            _executionLayers = new Dictionary<int, PenExecutionLayer>();

            ReflectionProvider.Invalidate();
        }

        public static T CreateAndActive<T>(params object[] args) where T : PaperObject
        {
            var type = typeof(T);
            var types = args.Select((obj) => obj.GetType()).ToArray();
            var cnstr = type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, types, null);

            if (cnstr == null)
            {
                throw new NotFoundConstructorException(type, types);
            }

            var instance = (T)cnstr.Invoke(args);
            instance.SetActive();

            return instance;
        }
        public static void CallCycle(float deltaTime)
        {
            if (!ReflectionProvider.IsPaperCallOverriden) return;

            foreach (var pair in _executionLayers)
            {
                pair.Value.OnComponentsUpdate(deltaTime);
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
            if (_componentInstances.TryGetValue(dataType, out var component)) return component;

            return default;
        }
        internal static PenExecutionLayer RegisterScriptableSystem<T>(PenSystem<T> component) where T : PaperObject
        {
            var dataType = typeof(T);
            if (_componentInstances.ContainsKey(dataType))
            {
                throw new ScriptComponentSystemDuplicate(component.GetType(), dataType);
            }
            else _componentInstances.Add(dataType, component);

            if (!_executionLayers.TryGetValue(component.ExecutionLayer, out var executionLayer))
            {
                executionLayer = new PenExecutionLayer();
                _executionLayers.Add(component.ExecutionLayer, executionLayer);
            }

            return executionLayer;
        }
    }
}