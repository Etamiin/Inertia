using Inertia.Logging;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Inertia
{
    public static class CommandExecutorManager
    {
        private static Regex _commandLineRegex;

        static CommandExecutorManager()
        {
            _commandLineRegex = new Regex(@"\""(.*?)\""|\S+");
        }

        public static IEnumerable<string> GetCommandNames => ReflectionProvider.GetCommandNames();

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
        public static bool TryExecute(string commandLine, ILogger logger, object? state)
        {
            var matches = _commandLineRegex.Matches(commandLine);

            if (matches.Count == 0) return false;

            var commandName = matches[0];
            var arguments = new string[matches.Count - 1];

            for (var i = 1; i < matches.Count; i++)
            {
                var match = matches[i];
                var sg = match.Groups[1];

                arguments[i - 1] = sg.Success ? sg.Value : match.Value;
            }

            return TryExecuteByName(commandName.Value, logger, state, arguments);
        }
        public static bool TryExecuteByName(string commandName, params string[] arguments)
        {
            return TryExecuteByName(commandName, null, null, arguments);
        }
        public static bool TryExecuteByName(string commandName, ILogger logger, params string[] arguments)
        {
            return TryExecuteByName(commandName, logger, null, arguments);
        }
        public static bool TryExecuteByName(string commandName, object? state, params string[] arguments)
        {
            return TryExecuteByName(commandName, null, state, arguments);
        }
        public static bool TryExecuteByName(string commandName, ILogger logger, object? state, params string[] arguments)
        {
            if (arguments.Length == 0) return false;

            var cmd = ReflectionProvider.CreateCommandExecutor(commandName, logger ?? NullLogger.Instance, state);
            if (cmd is null) return false;

            cmd.Execute(arguments);

            return true;
        }
    }
}