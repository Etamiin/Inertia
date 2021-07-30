using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Inertia.Realtime;

namespace Inertia.Internal
{
    internal static class RealtimeManager
    {
        #region Events

        internal static event BasicAction ScriptInTimeUpdate = () => { };
        private static event BasicAction Destroyer = () => {};

        #endregion

        #region Internal variables

        internal const int MaxExecutorScriptCount = 335;

        #endregion

        #region Private variables

        private static bool m_isInitialized;
        private static ScriptExecutorLayer m_currentLayer;
        private static List<ScriptExecutorLayer> m_executorLayers;

        #endregion

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
                    var currentMsUpdate = (int)clock.GetElapsedMilliseconds();

                    if (currentMsUpdate < targetMsUpdate)
                    {
                        Thread.Sleep(targetMsUpdate - currentMsUpdate);
                        currentMsUpdate = (int)clock.GetElapsedMilliseconds();
                    }

                    Script.DeltaTime = currentMsUpdate / 1000f;
                    Script.Time += Script.DeltaTime;

                    clock.Reset();

                    ScriptInTimeUpdate();
                    ScriptExecutorLayer[] updaters = null;

                    try
                    {
                        lock (m_executorLayers)
                            updaters = m_executorLayers.ToArray();

                        for (var i = 0; i < updaters.Length; i++)
                            updaters[i].Execute();

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

            var executorLayer = script.ExecutorLayer;

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
            script.Collection.FinalRemove(script);
        }
        internal static void OnRegisterExtends(ScriptInTime sit)
        {
            if (!m_isInitialized)
                Initialize();
        }
    }
}
