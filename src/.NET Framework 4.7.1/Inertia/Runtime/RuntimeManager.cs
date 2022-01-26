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

        internal static bool IsManuallyRunning
        {
            get
            {
                return _manuallyRunning;
            }
            set
            {
                _manuallyRunning = value;
                if (!_manuallyRunning && !_isInCycle) RunCycleLoop();
            }
        }

        private static bool _isInCycle;
        private static bool _manuallyRunning;

        static RuntimeManager()
        {
            RunCycleLoop();
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

        internal static void RunCycleLoop()
        {
            var currentState = _manuallyRunning;
            var clock = new Clock();
            Task.Factory.StartNew(() => {
                while (currentState == _manuallyRunning)
                {
                    _isInCycle = true;
                    ExecuteCycle(clock);
                    _isInCycle = false;
                }
            });
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

            RtUpdate?.Invoke();
            Updating?.Invoke();
            Destroying?.Invoke();
        }
    }
}