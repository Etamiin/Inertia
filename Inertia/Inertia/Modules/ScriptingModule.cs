using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Inertia
{
    public class ScriptingModule
    {
        internal static uint currentScriptId = 1;

        private static ScriptingModule _instance;
        public static ScriptingModule Module
        {
            get
            {
                if (_instance == null) {
                    _instance = new ScriptingModule();
                }
                return _instance;
            }
        }

        public static float DeltaTime { get; private set; }

        private LoopContext loopContext;

        private ContainerHandler startHandler = () => { };
        private ContainerHandler updateHandler = () => { };
        private ContainerHandler destroyHandler = () => { };

        internal ScriptingModule()
        {
            var clock = new Clock();

            loopContext = new LoopContext();
            loopContext.AddHandler(() => {
                if (DeltaTime < .00016f) {
                    Thread.Sleep(1);
                }

                lock (startHandler)
                    startHandler();

                lock (updateHandler)
                    updateHandler();

                lock (destroyHandler)
                    destroyHandler();

                DeltaTime = (float)clock.GetElapsedSecondsAndReset();
            });


            loopContext.Start();
        }

        internal static void GenerateScriptEvents(Scriptable script)
        {
            Module.startHandler += script.InternalStart;
            Module.updateHandler += script.InternalUpdate;
        }
        internal static void ScriptStarted(Scriptable script)
        {
            Module.startHandler -= script.InternalStart;
        }
        internal static void DeleteScript(Scriptable script)
        {
            if (script.DestroyRequested)
                return;

            Module.destroyHandler += script.DestroyCheck;
            script.DestroyRequested = true;
        }
        internal static void OnScriptDeleted(Scriptable script)
        {
            Module.destroyHandler -= script.DestroyCheck;
            Module.updateHandler -= script.InternalUpdate;
        }

    }
}
