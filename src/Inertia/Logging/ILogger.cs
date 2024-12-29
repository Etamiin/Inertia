namespace Inertia.Logging
{
    public interface ILogger 
    {
        public void Log(LogLevel level, object content);
        public void Debug(object content);
        public void Info(object content);
        public void Warn(object content);
        public void Error(object content);
        public bool IsLogEnabled(LogLevel level);
    }
}
