using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Inertia.Scriptable
{
    internal sealed class PenExecutionLayer
    {
        internal event BasicAction<float>? ComponentsUpdate;
        
        internal void OnComponentsUpdate(float deltaTime)
        {
            ComponentsUpdate?.Invoke(deltaTime);
        }

        internal PenExecutionLayer()
        {
            if (!ReflectionProvider.IsPaperCallOverriden)
            {
                Task.Factory.StartNew(() => ExecuteCycle(new Clock()), TaskCreationOptions.LongRunning);
            }
        }

        private void ExecuteCycle(Clock clock)
        {
            while (true)
            {
                var currentMsUpdate = clock.GetElapsedSeconds();
                var targetMsUpdate = 1000.0d / PaperFactory.TargetTickPerSecond;

                if (currentMsUpdate < targetMsUpdate)
                {
                    if (ComponentsUpdate == null || PaperFactory.LimitProcessorUsage) Thread.Sleep(1);
                    else
                    {
                        var msToSleep = (targetMsUpdate - currentMsUpdate) / 1000.0d;
                        var durationTicks = Math.Round(msToSleep * Stopwatch.Frequency);

                        while (clock.ElapsedTicks < durationTicks) { }
                    }

                    currentMsUpdate = clock.GetElapsedSeconds();
                }

                clock.Reset();
                ComponentsUpdate?.Invoke((float)currentMsUpdate);
            }
        }
    }
}
