using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Inertia.Runtime
{
    internal static class RuntimeManager
    {
        internal static bool IsManuallyRunned { get; set; } = false;

        internal static event BasicAction ScriptInTimeUpdate = () => { };
        private static event BasicAction Destroyer = () => { };

        private static bool _isInitialized;
        private static ScriptExecutorLayer _loopExecutor;

        internal static void OnRegisterExtends()
        {
            if (!_isInitialized)
                Initialize();
        }

        internal static void OnScriptCreated(Script script)
        {
            if (!_isInitialized)
                Initialize();

            _loopExecutor.Join(script);
        }
        internal static void OnScriptDestroyed(Script script)
        {
            if (!_isInitialized)
                Initialize();

            script.AttachedLayer.Leave(script);
            Destroyer += script.PreDestroy;
        }
        internal static void OnScriptPreDestroyed(Script script)
        {
            Destroyer -= script.PreDestroy;
            script.InCollection.FinalizeRemove(script);
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

            try
            {
                lock (ScriptInTimeUpdate)
                    ScriptInTimeUpdate();

                _loopExecutor.Execute();

                lock (Destroyer)
                    Destroyer?.Invoke();
            }
            catch { }
        }

        private static void Initialize()
        {
            if (_isInitialized) return;

            _loopExecutor = new ScriptExecutorLayer();
            _isInitialized = true;

            ExecuteLogic();
        }
        private static void ExecuteLogic()
        {
            var clock = new Clock();

            Task.Factory.StartNew(() => {
                while (true)
                {
                    if (IsManuallyRunned) break;

                    ExecuteCycle(clock);
                }
            });
        }
    }
}