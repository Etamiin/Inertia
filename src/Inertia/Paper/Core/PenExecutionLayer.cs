using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Inertia.Scriptable
{
    internal sealed class PenExecutionLayer : IDisposable
    {
        internal event BasicAction<float>? ComponentsUpdate;

        internal bool IsDisposed { get; private set; }
        
        internal PenExecutionLayer()
        {
            if (!ReflectionProvider.IsPaperCallOverriden)
            {
                Task.Factory.StartNew(() => ExecuteCycle(new Clock()), TaskCreationOptions.LongRunning);
            }
        }

        internal void OnComponentsUpdate(float deltaTime)
        {
            ComponentsUpdate?.Invoke(deltaTime);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void ExecuteCycle(Clock clock)
        {
            while (!IsDisposed)
            {
                var currentMsUpdate = clock.GetElapsedSeconds();
                var targetMsUpdate = 1000.0d / PaperFactory.TargetTickPerSecond;

                if (currentMsUpdate < targetMsUpdate)
                {
                    if (ComponentsUpdate == null || PaperFactory.LimitProcessorUsage) Thread.Sleep(1);
                    else
                    {
                        var msToSleep = (targetMsUpdate - currentMsUpdate) / 1000.0d;
                        var sleepTicks = Math.Round(msToSleep * Stopwatch.Frequency);

                        while (clock.ElapsedTicks < sleepTicks) { }
                    }

                    currentMsUpdate = clock.GetElapsedSeconds();
                }

                clock.Reset();
                ComponentsUpdate?.Invoke((float)currentMsUpdate);
            }
        }
        private void Dispose(bool disposing)
        {
            if (IsDisposed) return;

            if (disposing)
            {
                //
            }

            IsDisposed = true;
        }
    }
}