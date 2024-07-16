using System;
using System.Collections.Generic;
using System.Text;

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
        }
        public void Info(object content)
        {
        }
        public void Warn(object content)
        {
        }
        public void Error(object content)
        {
        }
    }
}
