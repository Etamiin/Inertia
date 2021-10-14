using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Inertia.Runtime
{
    internal static class RuntimeManager
    {
        internal static bool IsManuallyRunned { get; set; }

        internal static event BasicAction UpdatingSiT = () => { };
        private static event BasicAction Updating = () => { };
        private static event BasicAction Destroying = () => { };

        private static bool _isInitialized;

        internal static void OnRegisterExtends()
        {
            if (!_isInitialized)
            {
                Initialize();
            }
        }

        internal static void OnScriptCreated(Script script)
        {
            if (!_isInitialized)
            {
                Initialize();
            }

            Updating += script.Update;
        }
        internal static void OnScriptDestroyed(Script script)
        {
            if (!_isInitialized)
            {
                Initialize();
            }

            Updating -= script.Update;
            Destroying += script.PreDestroy;
        }
        internal static void OnScriptPreDestroyed(Script script)
        {
            Destroying -= script.PreDestroy;
            script.Parent.FinalizeRemove(script);
        }

        internal static void ExecuteCycle(Clock clock)
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

        private static void Initialize()
        {
            if (!_isInitialized)
            {
                _isInitialized = true;
                ExecuteLogic();
            }
        }
        private static void ExecuteLogic()
        {
            var clock = new Clock();

            Task.Factory.StartNew(() => {
                while (!IsManuallyRunned)
                {
                    ExecuteCycle(clock);
                }
            });
        }
    }
}