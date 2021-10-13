using System;
using System.Collections.Generic;
using System.Text;

namespace Inertia.Runtime
{
    /// <summary>
    /// 
    /// </summary>
    public static class Run
    {
        /// <summary>
        ///
        /// </summary>
        public sealed class ExecuteScriptIn
        {
            /// <summary>
            /// Returns true if the current script run permanently.
            /// </summary>
            public bool Permanent { get; set; }
            /// <summary>
            /// Returns true if the current script is running.
            /// </summary>
            public bool IsRunning { get; set; }

            private readonly BasicAction<ExecuteScriptIn> _action;
            private readonly float _time;
            private float _currentTime, _totalTime;

            internal ExecuteScriptIn(float time, BasicAction<ExecuteScriptIn> action) : this(time, action, false)
            {
            }
            internal ExecuteScriptIn(float time, BasicAction<ExecuteScriptIn> action, bool permanent)
            {
                _action = action;
                _time = time;
                Permanent = permanent;

                RuntimeManager.UpdatingSiT += Update;
                RuntimeManager.OnRegisterExtends();

                IsRunning = true;
            }
            internal ExecuteScriptIn(float time, BasicAction<ExecuteScriptIn> action, float totalTime) : this(time, action, false)
            {
                _totalTime = totalTime;
            }

            /// <summary>
            /// Stop the current script
            /// </summary>
            public void Stop()
            {
                if (IsRunning)
                {
                    RuntimeManager.UpdatingSiT -= Update;
                    IsRunning = false;
                }
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

                        Stop();
                    }
                }
            }
        }
        internal sealed class NextFrameExecution
        {
            private readonly BasicAction _action;

            internal NextFrameExecution(BasicAction action)
            {
                if (action != null)
                {
                    _action = action;

                    RuntimeManager.UpdatingSiT += Execute;
                    RuntimeManager.OnRegisterExtends();
                }
            }

            private void Execute()
            {
                _action?.Invoke();
                RuntimeManager.UpdatingSiT -= Execute;
            }
        }

        /// <summary>
        /// Execute the Runtime cycle manually.
        /// </summary>
        public static void ManualUpdate()
        {
            RuntimeManager.IsManuallyRunned = true;
            RuntimeManager.ExecuteCycle(null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="delayInSeconds"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static ExecuteScriptIn Delayed(float delayInSeconds, BasicAction<ExecuteScriptIn> callback)
        {
            return new ExecuteScriptIn(delayInSeconds, callback);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="delayInSeconds"></param>
        /// <param name="callback"></param>
        /// <param name="permanent"></param>
        /// <returns></returns>
        public static ExecuteScriptIn Delayed(float delayInSeconds, BasicAction<ExecuteScriptIn> callback, bool permanent)
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