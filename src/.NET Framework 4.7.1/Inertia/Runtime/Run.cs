using System;
using System.Collections.Generic;
using System.Text;

namespace Inertia.Runtime
{
<<<<<<< HEAD
<<<<<<< HEAD
    /// <summary>
    /// 
    /// </summary>
    public static class Run
    {
        /// <summary>
        ///
        /// </summary>
=======
    public static class Run
    {
>>>>>>> 9bfc85f6784b254a10c65f104446a83c8b195c40
=======
    public static class Run
    {
>>>>>>> premaster
        public sealed class ExecuteScriptIn
        {
            /// <summary>
            /// Returns true if the current script run permanently.
            /// </summary>
            public bool Permanent { get; set; }
<<<<<<< HEAD
<<<<<<< HEAD
            /// <summary>
            /// Returns true if the current script is running.
            /// </summary>
=======
>>>>>>> 9bfc85f6784b254a10c65f104446a83c8b195c40
            public bool IsRunning { get; set; }
=======
            public bool IsRunning { get; private set; }
>>>>>>> premaster

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

<<<<<<< HEAD
                RuntimeManager.UpdatingSiT += Update;
<<<<<<< HEAD
                RuntimeManager.OnRegisterExtends();
=======
>>>>>>> 9bfc85f6784b254a10c65f104446a83c8b195c40
=======
                RuntimeManager.RtUpdate += Update;
>>>>>>> premaster

                IsRunning = true;
            }
            internal ExecuteScriptIn(float time, BasicAction<ExecuteScriptIn> action, float totalTime) : this(time, action, false)
            {
                _totalTime = totalTime;
            }

<<<<<<< HEAD
<<<<<<< HEAD
            /// <summary>
            /// Stop the current script
            /// </summary>
=======
>>>>>>> 9bfc85f6784b254a10c65f104446a83c8b195c40
=======
>>>>>>> premaster
            public void Stop()
            {
                if (IsRunning)
                {
<<<<<<< HEAD
                    RuntimeManager.UpdatingSiT -= Update;
=======
                    RuntimeManager.RtUpdate -= Update;
>>>>>>> premaster
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

<<<<<<< HEAD
                    RuntimeManager.UpdatingSiT += Execute;
<<<<<<< HEAD
                    RuntimeManager.OnRegisterExtends();
=======
>>>>>>> 9bfc85f6784b254a10c65f104446a83c8b195c40
=======
                    RuntimeManager.RtUpdate += Execute;
>>>>>>> premaster
                }
            }

            private void Execute()
            {
                _action?.Invoke();
<<<<<<< HEAD
                RuntimeManager.UpdatingSiT -= Execute;
=======
                RuntimeManager.RtUpdate -= Execute;
>>>>>>> premaster
            }
        }

        /// <summary>
        /// Execute the Runtime cycle manually.
        /// </summary>
<<<<<<< HEAD
<<<<<<< HEAD
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
=======
        public static void ManualUpdate(float deltaTime)
        {
            RuntimeManager.IsManuallyRunned = true;
            RuntimeManager.ExecuteCycle(null, deltaTime);
        }

>>>>>>> 9bfc85f6784b254a10c65f104446a83c8b195c40
        public static ExecuteScriptIn Delayed(float delayInSeconds, BasicAction<ExecuteScriptIn> callback)
        {
            return new ExecuteScriptIn(delayInSeconds, callback);
        }
<<<<<<< HEAD
        /// <summary>
        /// 
        /// </summary>
        /// <param name="delayInSeconds"></param>
        /// <param name="callback"></param>
        /// <param name="permanent"></param>
        /// <returns></returns>
=======
>>>>>>> 9bfc85f6784b254a10c65f104446a83c8b195c40
        public static ExecuteScriptIn Delayed(float delayInSeconds, BasicAction<ExecuteScriptIn> callback, bool permanent)
        {
            return new ExecuteScriptIn(delayInSeconds, callback, permanent);
        }
<<<<<<< HEAD
        /// <summary>
        /// 
        /// </summary>
        /// <param name="delayInSeconds"></param>
        /// <param name="callback"></param>
        /// <param name="runningTime"></param>
        /// <returns></returns>
=======
>>>>>>> 9bfc85f6784b254a10c65f104446a83c8b195c40
        public static ExecuteScriptIn Delayed(float delayInSeconds, BasicAction<ExecuteScriptIn> callback, float runningTime)
        {
            return new ExecuteScriptIn(delayInSeconds, callback, runningTime);
        }
<<<<<<< HEAD
        /// <summary>
        /// 
        /// </summary>
        /// <param name="callback"></param>
=======
        
>>>>>>> 9bfc85f6784b254a10c65f104446a83c8b195c40
=======
        public static void ManualUpdate(float deltaTime)
        {
            RuntimeManager.IsManuallyRunning = true;
            RuntimeManager.ExecuteCycle(null, deltaTime);
        }

        public static void Delayed(float delayInSeconds, BasicAction<ExecuteScriptIn> callback)
        {
            new ExecuteScriptIn(delayInSeconds, callback);
        }
        public static void Delayed(float delayInSeconds, BasicAction<ExecuteScriptIn> callback, float runningTime)
        {
            new ExecuteScriptIn(delayInSeconds, callback, runningTime);
        }
        public static void DelayedLoop(float delayInSeconds, BasicAction<ExecuteScriptIn> callback)
        {
            new ExecuteScriptIn(delayInSeconds, callback, true);
        }
>>>>>>> premaster
        public static void ToNextFrame(BasicAction callback)
        {
            new NextFrameExecution(callback);
        }
<<<<<<< HEAD
=======
        
        public static T CreateScript<T>(params object[] dataCollection) where T : Script
        {
            var scriptType = typeof(T);
            var cstr = scriptType.GetConstructor(Type.EmptyTypes);
            if (cstr != null)
            {
                var instance = Activator.CreateInstance<T>();
                instance.OnInitialize(new ScriptArguments(dataCollection));

                RuntimeManager.RegisterScript(instance);
                return instance;
            }

            return null;
        }
>>>>>>> premaster
    }
}