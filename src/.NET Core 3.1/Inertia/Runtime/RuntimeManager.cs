using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Inertia.Runtime
{
    internal static class RuntimeManager
    {
        static RuntimeManager()
        {
            var clock = new Clock();

            Task.Factory.StartNew(() => {
                while (!IsManuallyRunned)
                {
                    ExecuteCycle(clock);
                }
            });
        }

        internal static bool IsManuallyRunned { get; set; }

        internal static event BasicAction UpdatingSiT = () => { };
        private static event BasicAction Updating = () => { };
        private static event BasicAction Destroying = () => { };

        internal static void OnScriptCreated(Script script)
        {
            Updating += script.Update;
        }
        internal static void OnScriptDestroyed(Script script)
        {
            Updating -= script.Update;
            Destroying += script.PreDestroy;
        }
        internal static void OnScriptPreDestroyed(Script script)
        {
            Destroying -= script.PreDestroy;
            script.Parent.FinalizeRemove(script);
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

            lock (UpdatingSiT)
            {
                UpdatingSiT?.Invoke();
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