using Inertia.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inertia
{
    /// <summary>
    /// Core component for command line management and execution.
    /// </summary>
    public static class CommandManager
    {
        public static IEnumerable<CommandLine> GetAllCommands => ReflectionProvider.GetAllCommands();

        public static bool TryExecute(string commandLine)
        {
            return TryExecute(commandLine, null, null);
        }
        public static bool TryExecute(string commandLine, ILogger logger)
        {
            return TryExecute(commandLine, logger, null);
        }
        public static bool TryExecute(string commandLine, object? state)
        {
            return TryExecute(commandLine, null, state);
        }
        public static bool TryExecute(string commandLine, ILogger? logger, object? state)
        {
            if (string.IsNullOrWhiteSpace(commandLine)) return false;

            var values = commandLine.Split(' ');
            var args = new string[values.Length - 1];
            Array.Copy(values, 1, args, 0, args.Length);

            return TryExecuteByName(values[0], logger, state, commandLine.Contains('"'), args);
        }
        
        private static bool TryExecuteByName(string commandName, ILogger? logger, object? state, bool containsQuotes, params string[] arguments)
        {
            if (ReflectionProvider.TryGetCommand(commandName, out CommandLine cmd))
            {
                if (logger == null)
                {
                    logger = LoggingProvider.Logger;
                }

                if (containsQuotes)
                {
                    var parsedArguments = new List<string>();
                    var argBuilder = new StringBuilder();

                    foreach (var arg in arguments)
                    {
                        if (arg.EndsWith('"'))
                        {
                            argBuilder.Append($" {arg.Remove(arg.Length - 1, 1)}");
                            parsedArguments.Add(argBuilder.ToString().Trim());
                            argBuilder.Clear();
                        }
                        else if (arg.StartsWith('"')) argBuilder.Append(arg.Remove(0, 1));
                        else if (argBuilder.Length > 0) argBuilder.Append($" {arg}");
                        else parsedArguments.Add(arg);
                    }

                    arguments = parsedArguments.ToArray();
                }

                cmd.Execute(new CommandLineArguments(arguments), logger, state);

                return true;
            }

            return false;
        }
    }
}