using Inertia.Logging;
using System;

namespace Inertia.Extensions
{
    public static class ILoggerExtensions
    {
        public static void Log(this ILogger logger, LogLevel level, object content, Type type, string methodName)
        {
            logger.Log(level, $"{type.Name}.{methodName}(): {content}");
        }
        public static void Debug(this ILogger logger, object content, Type type, string methodName)
        {
            logger.Debug($"{type.Name}.{methodName}(): {content}");
        }
        public static void Info(this ILogger logger, object content, Type type, string methodName)
        {
            logger.Info($"{type.Name}.{methodName}(): {content}");
        }
        public static void Warn(this ILogger logger, object content, Type type, string methodName)
        {
            logger.Warn($"{type.Name}.{methodName}(): {content}");
        }
        public static void Error(this ILogger logger, object content, Type type, string methodName)
        {
            logger.Error($"{type.Name}.{methodName}(): {content}");
        }
    }
}
