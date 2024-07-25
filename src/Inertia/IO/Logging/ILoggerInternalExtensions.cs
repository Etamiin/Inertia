using System;

namespace Inertia.Logging
{
    internal static class ILoggerInternalExtensions
    {
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
