using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Inertia.Runtime
{
    internal static class RuntimeManager
    {
        internal static event BasicAction RtUpdate = () => { };
        private static event BasicAction Updating = () => { };
        private static event BasicAction Destroying = () => { };

        internal static bool IsManuallyRunning { get; set; }

        static RuntimeManager()
        {
            var clock = new Clock();

            Task.Factory.StartNew(() => {
                while (!IsManuallyRunning)
                {
                    ExecuteCycle(clock);
                }
            });
        }

        internal static void RegisterScript(Script script)
        {
            Updating += script.OnUpdate;
        }
        internal static void BeginUnregisterScript(Script script)
        {
            Updating -= script.OnUpdate;
            Destroying += script.PreDestroy;
        }
        internal static void EndUnregisterScript(Script script)
        {
            Destroying -= script.PreDestroy;
        }

        internal static void ExecuteCycle(Clock clock, float deltaTime = 0f)
        {
            if (clock != null)
            {
                var currentMsUpdate = clock.GetElapsedSeconds();
                if (currentMsUpdate == 0)
                {
                    Thread.Sleep(1);
                    currentMsUpdate = clock.GetElapsedSeconds();
                }

                Script.DeltaTime = (float)currentMsUpdate;
                clock.Reset();
            }
            else
            {
                Script.DeltaTime = deltaTime;
            }

            lock (RtUpdate)
            {
                RtUpdate?.Invoke();
            }
            lock (Updating)
            {
                Updating?.Invoke();
            }
            lock (Destroying)
            {
                Destroying?.Invoke();
            }
        }
    }
}