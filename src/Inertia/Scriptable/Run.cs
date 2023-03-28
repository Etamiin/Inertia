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
            ScriptableObject.CreateAndActive<TimedScriptData>(action);
        }
        public static void Delayed(float delayTimeSeconds, BasicAction action)
        {
            ScriptableObject.CreateAndActive<TimedScriptData>(action, delayTimeSeconds);
        }
        public static void Delayed(TimeSpan delayTime, BasicAction action)
        {
            ScriptableObject.CreateAndActive<TimedScriptData>(action, delayTime);
        }
        public static TimedScriptData DelayedLoop(float delayTimeSeconds, BasicAction action)
        {
            return ScriptableObject.CreateAndActive<TimedScriptData>(action, delayTimeSeconds, true);
        }
        public static TimedScriptData DelayedLoop(TimeSpan delayTime, BasicAction action)
        {
            return ScriptableObject.CreateAndActive<TimedScriptData>(action, delayTime, true);
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