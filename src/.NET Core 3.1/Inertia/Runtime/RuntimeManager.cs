using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Inertia.Runtime
{
    internal static class RuntimeManager
    {
        internal static event BasicAction RtUpdate = () => { };

        private static event BasicAction Updating = () => { };
        private static event BasicAction Destroying = () => { };

        private static int _scriptRunningCount;

        static RuntimeManager()
        {
            RunCycleLoop();
        }

        internal static void IncrementScriptRunning()
        {
            Interlocked.Increment(ref _scriptRunningCount);
        }
        internal static void DecrementScriptRunning()
        {
            Interlocked.Decrement(ref _scriptRunningCount);
        }

        internal static void RegisterScript(Script script)
        {
            Updating += script.OnUpdate;
            IncrementScriptRunning();
        }
        internal static void BeginUnregisterScript(Script script)
        {
            Updating -= script.OnUpdate;
            Destroying += script.PreDestroy;
        }
        internal static void EndUnregisterScript(Script script)
        {
            Destroying -= script.PreDestroy;
            DecrementScriptRunning();            
        }

        internal static void RunCycleLoop()
        {
            var clock = new Clock();
            Task.Factory.StartNew(() => {
                while (!ReflectionProvider.IsRuntimeCallOverriden)
                {
                    ExecuteCycle(clock);
                }
            }, TaskCreationOptions.LongRunning);
        }
        internal static void ExecuteCycle(Clock clock, float deltaTime = 0f)
        {
            if (clock != null)
            {
                var currentMsUpdate = clock.GetElapsedSeconds();
                var targetMsUpdate = 1000.0d / Run.TargetTickPerSecond;

                if (currentMsUpdate < targetMsUpdate)
                {
                    var sToSleep = (targetMsUpdate - currentMsUpdate) / 1000.0d;
                    var durationTicks = Math.Round(sToSleep * Stopwatch.Frequency);

                    if (_scriptRunningCount == 0 || Run.LimitProcessorUsage) Thread.Sleep(1);

                    while (clock.ElapsedTicks < durationTicks) { }
                    currentMsUpdate = clock.GetElapsedSeconds();
                }

                Script.DeltaTime = (float)currentMsUpdate;
                clock.Reset();
            }
            else
            {
                Script.DeltaTime = deltaTime;
            }

            RtUpdate?.Invoke();
            Updating?.Invoke();
            Destroying?.Invoke();
        }
    }
}