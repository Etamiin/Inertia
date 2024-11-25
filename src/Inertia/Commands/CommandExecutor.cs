using Inertia.Logging;

namespace Inertia
{
    public abstract class CommandExecutor
    {
        public abstract string Name { get; }

        public ILogger Logger { get; internal set; }
        public object? State { get; internal set; }

        public abstract void Execute(string[] args);
    }
}