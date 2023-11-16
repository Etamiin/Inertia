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
        private static IEnumerable<BasicCommand> _commands;

        public static IEnumerable<BasicCommand> GetCommandList()
        {
            if (_commands == null)
            {
                _commands = ReflectionProvider.GetAllCommands();
            }

            return _commands;
        }

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
            if (ReflectionProvider.TryGetCommand(commandName, out BasicCommand cmd))
            {
                if (logger != null)
                {
                    cmd.Logger = logger;
                }

                if (containsQuotes)
                {
                    var parsedArguments = new List<string>();
                    var argBuilder = new StringBuilder();

                    foreach (var arg in arguments)
                    {
                        if (arg.EndsWith('"'))
                        {
                            argBuilder.Append($" {arg}");
                            parsedArguments.Add(argBuilder.Replace("\"", string.Empty).ToString().Trim());
                            argBuilder.Clear();
                        }
                        else if (arg.StartsWith('"')) argBuilder.Append(arg);
                        else if (argBuilder.Length > 0) argBuilder.Append($" {arg}");
                        else parsedArguments.Add(arg);
                    }

                    arguments = parsedArguments.ToArray();
                }

                cmd.OnExecute(new CommandArguments(arguments, state));

                return true;
            }

            return false;
        }
    }
}