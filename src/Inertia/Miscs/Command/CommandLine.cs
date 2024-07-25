using Inertia.Logging;
using System.Collections.Generic;
using System.Text;

namespace Inertia
{
    public abstract class CommandLine
    {
        public abstract string Name { get; }

        public ILogger Logger { get; set; }
        public object? State { get; set; }

        public CommandLine()
        {
        }

        public abstract void Execute(CommandLineArguments arguments);
    }
}