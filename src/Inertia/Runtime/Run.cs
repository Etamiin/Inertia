using Inertia.Plugins;
using System;

namespace Inertia.Scriptable
{
    public static class Run
    {
        private const int MaxTickPerSecond = 180;

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

        private static int _targetTickPerSecond = MaxTickPerSecond;

        /// <summary>
        /// Execute the Runtime cycle manually.
        /// </summary>
        public static void RuntimeCall(float deltaTime)
        {
            RuntimeManager.RuntimeCall(deltaTime);
        }

        public static void OnNextTick(BasicAction action)
        {
            new TimedScriptData(action);
        }
        public static void Delayed(float delayTimeSeconds, BasicAction action)
        {
            new TimedScriptData(action, delayTimeSeconds);
        }
        public static TimedScriptData DelayedLoop(float delayTimeSeconds, BasicAction action)
        {
            return new TimedScriptData(action, delayTimeSeconds, true);
        }
        public static void Delayed(TimeSpan delayTime, BasicAction action)
        {
            new TimedScriptData(action, delayTime);
        }
        public static TimedScriptData DelayedLoop(TimeSpan delayTime, BasicAction action)
        {
            return new TimedScriptData(action, delayTime, true);
        }

        public static PluginExecutionResult TryStartPlugin(string pluginFilePath, params object[] executionParameters)
        {
            return ReflectionProvider.TryStartPlugin(pluginFilePath, executionParameters);
        }
        public static bool TryStopPlugin(string pluginIdentifier)
        {
            return ReflectionProvider.TryStopPlugin(pluginIdentifier);
        }
    }
}