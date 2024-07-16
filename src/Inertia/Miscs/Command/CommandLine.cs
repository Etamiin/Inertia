using Inertia.Logging;
using System.Collections.Generic;
using System.Text;

namespace Inertia
{
    public abstract class CommandLine
    {
        public abstract string Name { get; }

        public abstract void Execute(CommandLineArguments arguments, ILogger logger, object? state);
    }
}