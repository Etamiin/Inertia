using System;

namespace Inertia.Logging
{
    public interface ILogger 
    {
        public void Debug(object content);
        public void Info(object content);
        public void Warn(object content);
        public void Error(object content);
    }

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
