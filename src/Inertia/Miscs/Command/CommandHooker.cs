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
            return ReflectionProvider.GetAllCommands();
        }

        public static bool TryExecute(string commandLine, params object[] dataCollection)
        {
            var values = commandLine.Split(' ');
            var args = new string[values.Length - 1];
            Array.Copy(values, 1, args, 0, args.Length);

            return TryExecuteByName(values[0], dataCollection, commandLine.Contains('"'), args);
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