using Inertia.Logging;
using System.Collections.Generic;
using System.Text;

namespace Inertia
{
    public abstract class BasicCommand
    {
        public ILogger Logger { get; set; } = BasicLogger.Default;
        
        public abstract string Name { get; }
        
        public abstract void OnExecute(CommandArguments arguments);
    }
}