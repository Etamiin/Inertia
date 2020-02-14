using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Inertia
{
    public static class Logger
    {
        #region Events

        public static event LoggerHandler LogInfo = (log) => Console.WriteLine(log);
        public static event LoggerHandler LogError = (error) => Console.WriteLine(error);

        #endregion

        public static void Log(object log)
        {
            Log(GetObjectString(log));
        }
        public static void Log(string log, params object[] values)
        {
            if (!InertiaConfiguration.IsLogActive)
                return;

            for (var i = 0; i < values.Length; i++)
                log = log.Replace("{" + i + "}", GetObjectString(values[i]));

            LogInfo(log);
        }

        public static void Error(object error)
        {
            Error(GetObjectString(error));
        }
        public static void Error(string error, params object[] values)
        {
            if (!InertiaConfiguration.IsLogActive)
                return;

            for (var i = 0; i < values.Length; i++)
                error = error.Replace("{" + i + "}", GetObjectString(values[i]));

            LogError(error);
        }

        private static string GetObjectString(object value)
        {
            if (!value.GetType().IsArray)
                return value.ToString();
            else
            {
                var result = string.Empty;
                var array = (Array)value;

                for (var i = 0; i < array.Length; i++)
                {
                    result += array.GetValue(i).ToString();

                    if (i < array.Length - 1)
                        result += ", ";
                }

                return result;
            }
        }
    }
}
