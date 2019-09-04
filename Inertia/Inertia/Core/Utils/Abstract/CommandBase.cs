using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia
{
    public abstract class CommandBase
    {
        public virtual string Description { get; } = "No description";
        public abstract string Name { get; }

        public abstract void Execute(CommandArguments arguments);
        public virtual string GetHelp()
        {
            return $"No help for [{ Name }] command";
        }
    }
}
