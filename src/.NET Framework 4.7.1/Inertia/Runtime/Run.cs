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

                RuntimeManager.IncrementScriptRunning();
                RuntimeManager.RtUpdate += Update;

                IsRunning = true;
            }
            internal ExecuteScriptIn(float time, BasicAction<ExecuteScriptIn> action, float runningTime) : this(time, action, false)
            {
                _totalTime = runningTime;
            }

            public void Stop()
            {
                if (IsRunning)
                {
                    RuntimeManager.RtUpdate -= Update;
                    RuntimeManager.DecrementScriptRunning();

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
                        if (_totalTime <= _time)
                        {
                            Stop();
                        }
                        else
                        {
                            _totalTime -= _time;
                        }
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

                    RuntimeManager.IncrementScriptRunning();
                    RuntimeManager.RtUpdate += Execute;
                }
            }

            private void Execute()
            {
                _action?.Invoke();
                RuntimeManager.RtUpdate -= Execute;
                RuntimeManager.DecrementScriptRunning();
            }
        }

        public static bool LimitProcessorUsage { get; set; } = false;
        public static int TargetTickPerSecond { get; set; } = 120;

        /// <summary>
        /// Execute the Runtime cycle manually.
        /// </summary>
        public static void RuntimeCall(float deltaTime)
        {
            if (!ReflectionProvider.IsRuntimeCallOverriden) return;

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
        public static void InNextFrame(BasicAction callback)
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