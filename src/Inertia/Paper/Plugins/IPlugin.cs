using System;

namespace Inertia.Plugins
{
    public interface IPlugin
    {
        public string Identifier { get; }
        public bool LongRun { get; }
        public bool UsePaper { get; }
        public bool StopOnCatchedError { get; }
        
        public void Execute(object[] parameters);
        public void Stopping();
        public void Error(Exception exception);
    }
}