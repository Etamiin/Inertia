using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Inertia.Runtime
{
    internal static class RuntimeManager
    {
        internal static event BasicAction ScriptInTimeUpdate = () => { };
        private static event BasicAction Destroyer = () => { };

        private static bool m_isInitialized;
        private static ScriptExecutorLayer m_currentLayer;
        private static List<ScriptExecutorLayer> m_executorLayers;

        private static void Initialize()
        {
            if (m_isInitialized)
                return;

            m_executorLayers = new List<ScriptExecutorLayer>();
            m_isInitialized = true;

            ExecuteLogic();
        }
        private static void ExecuteLogic()
        {
            if (Script.MaxExecutionPerSecond > Script.FixedMaxExecutionPerSecond)
                Script.MaxExecutionPerSecond = Script.FixedMaxExecutionPerSecond;

            var targetMsUpdate = (int)Math.Round(1000f / Script.MaxExecutionPerSecond);
            var clock = new Clock();

            Task.Factory.StartNew(() => {
                while (true)
                {
                    var currentMsUpdate = clock.GetElapsedMilliseconds();

                    if (currentMsUpdate < targetMsUpdate)
                    {
                        Thread.Sleep(targetMsUpdate - (int)currentMsUpdate);
                        currentMsUpdate = clock.GetElapsedMilliseconds();
                    }

                    Script.DeltaTime = currentMsUpdate / 1000f;
                    clock.Reset();

                    try
                    {
                        ScriptInTimeUpdate();
                        ScriptExecutorLayer[] executors = null;

                        lock (m_executorLayers)
                            executors = m_executorLayers.ToArray();
                        
                        foreach (var executor in executors)
                            executor.Execute();

                        lock (Destroyer)
                            Destroyer?.Invoke();
                    }
                    catch { }
                }
            });
        }

        internal static void OnScriptCreated(Script script)
        {
            if (!m_isInitialized)
                Initialize();

            if (m_currentLayer == null || m_currentLayer.IsDisposed || m_currentLayer.LimitAchieved)
            {
                m_currentLayer = new ScriptExecutorLayer();
                m_executorLayers.Add(m_currentLayer);
            }

            m_currentLayer.Join(script);
        }
        internal static void OnScriptDestroyed(Script script)
        {
            if (!m_isInitialized)
                Initialize();

            var executorLayer = script.AttachedLayer;

            executorLayer.Leave(script);
            lock (m_executorLayers)
            {
                if (executorLayer.Count == 0)
                {
                    executorLayer.Dispose();
                    m_executorLayers.Remove(executorLayer);
                }
            }

            Destroyer += script.PreDestroy;
        }
        internal static void OnScriptPreDestroyed(Script script)
        {
            Destroyer -= script.PreDestroy;
            script.InCollection.FinalizeRemove(script);
        }
        internal static void OnRegisterExtends()
        {
            if (!m_isInitialized)
                Initialize();
        }
    }
}
