using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.ORM
{
    public static class LoggerORM
    {
        #region Events

        public static event OrmErrorHandler OnError = (error) => Console.WriteLine(error);

        #endregion

        public static void Error(object error)
        {
            Error(GetObjectString(error));
        }
        public static void Error(string error, params object[] values)
        {
            for (var i = 0; i < values.Length; i++)
                error = error.Replace("{" + i + "}", GetObjectString(values[i]));

            OnError(error);
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
