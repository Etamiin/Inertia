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

        public readonly static ScriptingManager Current = new ScriptingManager();

        #endregion

        #region Static variables

        public static float DeltaTime { get; private set; }

        #endregion

        #region Events

        private event InertiaAction StartHandler = () => { };
        private event InertiaAction DestroyHandler = () => { };

        #endregion

        #region Public variables

        public static ScriptCollection DefaultCollection = new ScriptCollection();

        #endregion

        #region Private variables

        private readonly DependentThread ThreadContext;
        private Updater CurrentUpdater;
        private readonly List<Updater> Updaters;
        private int ExecutedContainerCount;
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
            var script = DefaultCollection.Add<ExecuteInScript>();

            script.Time = Math.Abs(seconds);
            script.Action = action;
        }

        internal static void GenerateScriptEvents(Scriptable script)
        {
            var currentUpdateContainer = Current.CurrentUpdater;
            if (currentUpdateContainer == null || currentUpdateContainer.IsDisposed || currentUpdateContainer.Count >= InertiaConfiguration.MaxUpdaterScriptCount) {
                currentUpdateContainer = new Updater();
                Current.Updaters.Add(currentUpdateContainer);
                Current.CurrentUpdater = currentUpdateContainer;
            }

            Current.StartHandler += script.InternalStart;

            script.Updater = currentUpdateContainer;
            currentUpdateContainer.AddHandler(script.InternalUpdate);
        }
        internal static void ScriptStarted(Scriptable script)
        {
            Current.StartHandler -= script.InternalStart;
            Current.ScriptCount++;

            if (Current.ScriptCount >= InertiaConfiguration.CriticalScriptCount)
                throw new CriticalScriptManagement();
        }
        internal static void DeleteScript(Scriptable script)
        {
            if (script.DestroyRequested)
                return;

            Current.DestroyHandler += script.DestroyCheck;
            script.DestroyRequested = true;
        }
        internal static void OnScriptDeleted(Scriptable script)
        {
            Current.DestroyHandler -= script.DestroyCheck;
            script.Updater.RemoveHandler(script.InternalUpdate);

            Current.ScriptCount--;
        }
    
        internal static void ContainerExecuted(Updater updater)
        {
            Current.ExecutedContainerCount++;
        }
        internal static void RemoveContainer(Updater updater)
        {
            Current.Updaters.Remove(updater);
        }
    
        private void ExecuteLogic(Thread dependency)
        {
            var currentMsUpdate = 0;
            var targetMsUpdate = (int)(1f / InertiaConfiguration.MaxScriptExecutionPerSecond * 1000);
            var clock = Clock.Create();

            while (dependency.IsAlive)
            {
                currentMsUpdate = (int)clock.GetElapsedMillisecondsAndReset();
                if (currentMsUpdate < targetMsUpdate)
                {
                    Thread.Sleep(targetMsUpdate - currentMsUpdate);
                    currentMsUpdate = (int)clock.GetElapsedMillisecondsAndReset();
                }
                DeltaTime = currentMsUpdate / 1000f;

                lock (StartHandler)
                    StartHandler();

                ExecutedContainerCount = 0;
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

                foreach (var updater in updaters)
                    Task.Factory.StartNew(updater.Execute);

                while (ExecutedContainerCount < updaters.Length) { Thread.Sleep(1); }

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
