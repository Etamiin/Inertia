using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Inertia.Runtime.Core
{
    internal sealed class ScriptExecutionLayer
    {
        internal event BasicAction<float>? ComponentsUpdate;

        internal ScriptExecutionLayer()
        {
            if (!ReflectionProvider.IsRuntimeCallOverriden)
            {
                Task.Factory.StartNew(() => ExecuteCycle(new Clock()), TaskCreationOptions.LongRunning);
            }
        }

        internal void ExecuteCycle(Clock clock)
        {
            while (true)
            {
                var currentMsUpdate = clock.GetElapsedSeconds();
                var targetMsUpdate = 1000.0d / Run.TargetTickPerSecond;

                if (currentMsUpdate < targetMsUpdate)
                {
                    var sToSleep = (targetMsUpdate - currentMsUpdate) / 1000.0d;
                    var durationTicks = Math.Round(sToSleep * Stopwatch.Frequency);

                    if (ComponentsUpdate == null || Run.LimitProcessorUsage) Thread.Sleep(1);

                    while (clock.ElapsedTicks < durationTicks) { }
                    currentMsUpdate = clock.GetElapsedSeconds();
                }

                clock.Reset();
                ComponentsUpdate?.Invoke((float)currentMsUpdate);
            }
        }
        internal void ExecuteCycle(float deltaTime)
        {
            ComponentsUpdate?.Invoke(deltaTime);
        }
    }
}
