using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inertia.Internal;
using Inertia.Realtime;

namespace Inertia.Realtime
{
    /// <summary>
    /// Represent a class that offers scripting execution in specified time
    /// </summary>
    public class ScriptInTime
    {
        /// <summary>
        /// Return true if the current <see cref="ScriptInTime"/> run permanently
        /// </summary>
        public bool Permanent { get; set; }

        private BasicAction m_action;
        private float m_time;
        private float m_current;

        /// <summary>
        /// Instantiate a new instance of the class <see cref="ScriptInTime"/>
        /// </summary>
        /// <param name="time">Time between each execution</param>
        /// <param name="action">Action to execute</param>
        /// <param name="permanent">True if permanent execution</param>
        public ScriptInTime(float time, BasicAction action, bool permanent = false)
        {
            m_action = action;
            m_time = time;
            Permanent = permanent;

            RealtimeManager.ScriptInTimeUpdate  += Update;
        }

        private void Update()
        {
            m_current += Script.DeltaTime;

            if (m_current >= m_time)
            {
                m_current -= m_time;
                try
                {
                    m_action();

                    if (!Permanent)
                        RealtimeManager.ScriptInTimeUpdate -= Update;
                }
                catch
                {
                    RealtimeManager.ScriptInTimeUpdate -= Update;
                }
            }
        }
    }
}
