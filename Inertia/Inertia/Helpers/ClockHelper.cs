using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia
{
    public static class ClockHelper
    {
        public static float DeltaTime
        {
            get
            {
                return ScriptingModule.DeltaTime;
            }
        }

        public static double GetElapsedMilliseconds(ActionHandler handler)
        {
            var clock = new Clock();
            handler();
            return clock.GetElapsedMilliseconds();
        }
        public static double GetElapsedSeconds(ActionHandler handler)
        {
            var clock = new Clock();
            handler();
            return clock.GetElapsedSeconds();
        }
    }
}
