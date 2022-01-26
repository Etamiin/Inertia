using System;
using System.Collections.Generic;
using System.Text;

namespace Inertia.Runtime
{
    public static class Run
    {
        public sealed class ExecuteScriptIn
        {
            /// <summary>
            /// Returns true if the current script run permanently.
            /// </summary>
            public bool Permanent { get; set; }
            public bool IsRunning { get; private set; }

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

                RuntimeManager.RtUpdate += Update;

                IsRunning = true;
            }
            internal ExecuteScriptIn(float time, BasicAction<ExecuteScriptIn> action, float totalTime) : this(time, action, false)
            {
                _totalTime = totalTime;
            }

            public void Stop()
            {
                if (IsRunning)
                {
                    RuntimeManager.RtUpdate -= Update;
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

                    RuntimeManager.RtUpdate += Execute;
                }
            }

            private void Execute()
            {
                _action?.Invoke();
                RuntimeManager.RtUpdate -= Execute;
            }
        }

        public static void EnterManualMode()
        {
            RuntimeManager.IsManuallyRunning = true;
        }
        public static void LeaveManualMode()
        {
            RuntimeManager.IsManuallyRunning = false;
        }

        /// <summary>
        /// Execute the Runtime cycle manually.
        /// </summary>
        public static void ManualCall(float deltaTime)
        {
            if (!RuntimeManager.IsManuallyRunning) return;

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
        public static void ToNextFrame(BasicAction callback)
        {
            new NextFrameExecution(callback);
        }
        
        public static T CreateScript<T>(params object[] parameters) where T : Script
        {
            var scriptType = typeof(T);
            var cstr = scriptType.GetConstructor(Type.EmptyTypes);
            if (cstr != null)
            {
                var instance = Activator.CreateInstance<T>();
                instance.OnInitialize(new ScriptArguments(parameters));

                RuntimeManager.RegisterScript(instance);
                return instance;
            }

            return null;
        }
    }
}