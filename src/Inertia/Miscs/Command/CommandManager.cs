using System;
using System.Collections.Generic;

namespace Inertia
{
    /// <summary>
    /// Core component for command line management and execution.
    /// </summary>
    public static class CommandManager
    {
        public static IEnumerable<BasicCommand> GetCommandList()
        {
            return ReflectionProvider.GetAllCommands();
        }

        public static bool TryExecute(string commandLine)
        {
            return TryExecute(commandLine, null);
        }
        public static bool TryExecute(string commandLine, object? state)
        {
            if (string.IsNullOrWhiteSpace(commandLine)) return false;

            var values = commandLine.Split(' ');
            var args = new string[values.Length - 1];
            Array.Copy(values, 1, args, 0, args.Length);

            return TryExecuteByName(values[0], state, commandLine.Contains('"'), args);
        }
        
        private static bool TryExecuteByName(string commandName, object? state, bool containsQuotes, params string[] arguments)
        {
            if (ReflectionProvider.TryGetCommand(commandName, out BasicCommand cmd))
            {
                BasicCommand.PreExecute(cmd, arguments, state, containsQuotes);
                return true;
            }

            return false;
        }
    }
}