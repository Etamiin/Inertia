namespace Inertia.Runtime
{
    /// <summary>
    ///
    /// </summary>
    public sealed class ScriptInTime
    {
        /// <summary>
        /// Returns true if the current instance run permanently.
        /// </summary>
        public bool Permanent { get; set; }

        private BasicAction<ScriptInTime> m_action;
        private float m_time;
        private float m_current;

        /// <summary>
        /// Instantiate a new instance of the class <see cref="ScriptInTime"/>
        /// </summary>
        /// <param name="time">Time between each execution</param>
        /// <param name="action">Action to execute</param>
        /// <param name="permanent">True if it will run permanently</param>
        public ScriptInTime(float time, BasicAction<ScriptInTime> action, bool permanent = false)
        {
            m_action = action;
            m_time = time;
            Permanent = permanent;

            RuntimeManager.ScriptInTimeUpdate += Update;
            RuntimeManager.OnRegisterExtends(this);
        }

        private void Update()
        {
            m_current += Script.DeltaTime;

            if (m_current >= m_time)
            {
                m_current -= m_time;
                try
                {
                    m_action(this);

                    if (!Permanent)
                        RuntimeManager.ScriptInTimeUpdate -= Update;
                }
                catch
                {
                    RuntimeManager.ScriptInTimeUpdate -= Update;
                }
            }
        }
    }
}
