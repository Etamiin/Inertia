using System;
using System.Collections.Generic;
using System.Text;

namespace Inertia.Runtime
{
    /// <summary>
    /// 
    /// </summary>
    public static partial class Run
    {
        /// <summary>
        ///
        /// </summary>
        public sealed class ExecuteScriptIn
        {
            /// <summary>
            /// Returns true if the current instance run permanently.
            /// </summary>
            public bool Permanent { get; set; }

            private BasicAction<ExecuteScriptIn> _action;
            private float _time, _currentTime, _totalTime;

            internal ExecuteScriptIn(float time, BasicAction<ExecuteScriptIn> action, bool permanent = false)
            {
                _action = action;
                _time = time;
                Permanent = permanent;

                RuntimeManager.ScriptInTimeUpdate += Update;
                RuntimeManager.OnRegisterExtends();
            }
            internal ExecuteScriptIn(float time, BasicAction<ExecuteScriptIn> action, float totalTime) : this(time, action, false)
            {
                _totalTime = totalTime;
            }

            private void Update()
            {
                _currentTime += Script.DeltaTime;

                if (_currentTime >= _time)
                {
                    _currentTime -= _time;
                    _action(this);

                    if (!Permanent)
                    {
                        if (_totalTime > _time)
                        {
                            _totalTime -= _time;
                            return;
                        }

                        RuntimeManager.ScriptInTimeUpdate -= Update;
                    }
                }
            }
        }
        internal sealed class NextFrameExecution
        {
            private BasicAction _action;

            internal NextFrameExecution(BasicAction action)
            {
                _action = action;

                RuntimeManager.ScriptInTimeUpdate += Execute;
                RuntimeManager.OnRegisterExtends();
            }

            private void Execute()
            {
                _action?.Invoke();
                RuntimeManager.ScriptInTimeUpdate -= Execute;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static void Update()
        {
            RuntimeManager.IsManuallyRunned = true;
            RuntimeManager.ExecuteCycle(null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="delayInSeconds"></param>
        /// <param name="callback"></param>
        /// <param name="permanent"></param>
        /// <returns></returns>
        public static ExecuteScriptIn Delayed(float delayInSeconds, BasicAction<ExecuteScriptIn> callback, bool permanent = false)
        {
            return new ExecuteScriptIn(delayInSeconds, callback, permanent);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="delayInSeconds"></param>
        /// <param name="callback"></param>
        /// <param name="runningTime"></param>
        /// <returns></returns>
        public static ExecuteScriptIn Delayed(float delayInSeconds, BasicAction<ExecuteScriptIn> callback, float runningTime)
        {
            return new ExecuteScriptIn(delayInSeconds, callback, runningTime);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="callback"></param>
        public static void ToNextFrame(BasicAction callback)
        {
            new NextFrameExecution(callback);
        }
    }
}