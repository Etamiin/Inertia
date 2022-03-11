using System;
using System.Collections.Generic;
using System.Linq;

namespace Inertia
{
    /// <summary>
    /// Core component for command line management and execution.
    /// </summary>
    public static class CommandHooker
    {
        public static BasicCommand[] GetCommandList()
        {
            return ReflectionProvider.GetCommandList();
        }

        public static bool TryExecute(string commandLine, params object[] dataCollection)
        {
            var args = commandLine.Split(' ');
            var others = new string[args.Length - 1];
            Array.Copy(args, 1, others, 0, others.Length);

            return TryExecuteByName(args[0], dataCollection, commandLine.Contains('"'), others);
        }
        
        private static bool TryExecuteByName(string commandName, object[] dataCollection, bool containsBlock, params string[] arguments)
        {
            if (ReflectionProvider.TryGetCommand(commandName, out BasicCommand cmd))
            {
                BasicCommand.PreExecute(cmd, arguments, dataCollection, containsBlock);
                return true;
            }

            return false;
        }
    }
}
