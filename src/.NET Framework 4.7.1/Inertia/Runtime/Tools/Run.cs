using System;
using System.Collections.Generic;
using System.Text;

namespace Inertia.Runtime
{
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

            private BasicAction<ExecuteScriptIn> m_action;
            private float m_time, m_currentTime, m_totalTime;

            internal ExecuteScriptIn(float time, BasicAction<ExecuteScriptIn> action, bool permanent = false)
            {
                m_action = action;
                m_time = time;
                Permanent = permanent;

                RuntimeManager.ScriptInTimeUpdate += Update;
                RuntimeManager.OnRegisterExtends();
            }
            internal ExecuteScriptIn(float time, BasicAction<ExecuteScriptIn> action, float totalTime) : this(time, action, false)
            {
                m_totalTime = totalTime;
            }

            private void Update()
            {
                m_currentTime += Script.DeltaTime;

                if (m_currentTime >= m_time)
                {
                    m_currentTime -= m_time;
                    m_action(this);

                    if (!Permanent)
                    {
                        if (m_totalTime > m_time)
                        {
                            m_totalTime -= m_time;
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

        public static ExecuteScriptIn Delayed(float delayInSeconds, BasicAction<ExecuteScriptIn> callback, bool permanent = false)
        {
            return new ExecuteScriptIn(delayInSeconds, callback, permanent);
        }
        public static ExecuteScriptIn Delayed(float delayInSeconds, BasicAction<ExecuteScriptIn> callback, float runningTime)
        {
            return new ExecuteScriptIn(delayInSeconds, callback, runningTime);
        }

        public static void ToNextFrame(BasicAction callback)
        {
            new NextFrameExecution(callback);
        }
    }
}