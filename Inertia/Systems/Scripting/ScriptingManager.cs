using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Inertia.Internal;

namespace Inertia.Scripting
{
    public class ScriptingManager
    {
        #region GetInstance

        public static ScriptingManager GetInstance()
        {
            if (Instance == null)
                Instance = new ScriptingManager();

            return Instance;
        }
        private static ScriptingManager Instance;

        #endregion

        #region Static variables

        public static float DeltaTime { get; private set; }

        #endregion

        #region Events

        private event InertiaAction StartHandler = () => { };
        private event InertiaAction DestroyHandler = () => { };

        #endregion

        #region Public variables

        public static ScriptCollection GetDefaultCollection()
        {
            if (CollectionInstance == null)
                CollectionInstance = new ScriptCollection();

            return CollectionInstance;
        }
        private static ScriptCollection CollectionInstance;

        #endregion

        #region Private variables

        private readonly DependentThread ThreadContext;
        private Updater CurrentUpdater;
        private readonly List<Updater> Updaters;
        private int ScriptCount;

        private readonly Random Randomizer;
        private readonly List<int> UsedIds;

        #endregion

        #region Constructors

        internal ScriptingManager()
        {
            Randomizer = new Random();
            UsedIds = new List<int>();

            Updaters = new List<Updater>();
            ThreadContext = new DependentThread((dependency) => ExecuteLogic(dependency));

            ThreadContext.Start();
        }

        #endregion

        public static void ExecuteIn(float seconds, InertiaAction action)
        {
            var script = GetDefaultCollection().Add<ExecuteInScript>();

            script.Time = Math.Abs(seconds);
            script.Action = action;
        }

        internal static void GenerateScriptEvents(Scriptable script)
        {
            var currentUpdateContainer = GetInstance().CurrentUpdater;
            if (currentUpdateContainer == null || currentUpdateContainer.IsDisposed || currentUpdateContainer.Count >= InertiaConfiguration.MaxUpdaterScriptCount) {
                currentUpdateContainer = new Updater();
                GetInstance().Updaters.Add(currentUpdateContainer);
                GetInstance().CurrentUpdater = currentUpdateContainer;
            }

            GetInstance().StartHandler += script.InternalStart;

            script.Updater = currentUpdateContainer;
            currentUpdateContainer.AddHandler(script.InternalUpdate);
        }
        internal static void ScriptStarted(Scriptable script)
        {
            GetInstance().StartHandler -= script.InternalStart;
            GetInstance().ScriptCount++;

            if (GetInstance().ScriptCount >= InertiaConfiguration.CriticalScriptCount)
                throw new CriticalScriptManagement();
        }
        internal static void DeleteScript(Scriptable script)
        {
            if (script.DestroyRequested)
                return;

            GetInstance().DestroyHandler += script.DestroyCheck;
            script.DestroyRequested = true;
        }
        internal static void OnScriptDeleted(Scriptable script)
        {
            GetInstance().DestroyHandler -= script.DestroyCheck;
            script.Updater.RemoveHandler(script.InternalUpdate);

            GetInstance().ScriptCount--;
        }
    
        internal static void RemoveContainer(Updater updater)
        {
            GetInstance().Updaters.Remove(updater);
        }
    
        private void ExecuteLogic(Thread dependency)
        {
            var targetMsUpdate = (int)Math.Round(1f / InertiaConfiguration.MaxScriptExecutionPerSecond * 1000);
            var clock = Clock.Create();

            while (dependency.IsAlive)
            {
                var currentMsUpdate = (int)clock.GetElapsedMilliseconds();

                if (currentMsUpdate < targetMsUpdate)
                {
                    Thread.Sleep(targetMsUpdate - currentMsUpdate);
                    currentMsUpdate = (int)clock.GetElapsedMilliseconds();
                }
                
                DeltaTime = currentMsUpdate / 1000f;
                clock.Reset();

                lock (StartHandler)
                    StartHandler();

                Updater[] updaters = null;

                try
                {
                    lock (Updaters)
                        updaters = Updaters.ToArray();
                }
                catch
                {
                    continue;
                }

                for (var i = 0; i < updaters.Length; i++)
                    updaters[i].Execute();

                lock (DestroyHandler)
                    DestroyHandler();
            }

            clock.Dispose();
        }
        internal int GenerateScriptId()
        {
            var id = Randomizer.Next(0, int.MaxValue - 1);
            if (UsedIds.Contains(id))
                return GenerateScriptId();
            else
                UsedIds.Add(id);

            return id;
        }
    }
}
