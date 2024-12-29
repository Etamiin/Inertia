using Inertia.Logging;

namespace Inertia
{
    public abstract class CommandHandler
    {
        public abstract string CommandName { get; }

        protected ILogger Logger { get; }
        protected object? State { get; }

        public CommandHandler(ILogger logger, object? state)
        {
            Logger = logger;
            State = state;
        }

        public abstract void Handle(string[] args);
    }
}