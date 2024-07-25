using System;

namespace Inertia.Logging
{
    public class NullLogger : ILogger
    {
        public static NullLogger Instance => lazy.Value;

        private static readonly Lazy<NullLogger> lazy = new Lazy<NullLogger>(() => new NullLogger());

        internal NullLogger()
        {
        }

        public void Debug(object content)
        {
            //No implementation for NullLogger
        }
        public void Info(object content)
        {
            //No implementation for NullLogger
        }
        public void Warn(object content)
        {
            //No implementation for NullLogger
        }
        public void Error(object content)
        {
            //No implementation for NullLogger
        }
    }
}
