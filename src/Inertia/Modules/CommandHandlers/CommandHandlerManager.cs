using Inertia.Logging;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Inertia
{
    public static class CommandHandlerManager
    {
        private static Regex _commandLineRegex;

        static CommandHandlerManager()
        {
            _commandLineRegex = new Regex(@"\""(.*?)\""|\S+");
        }

        public static IEnumerable<string> GetAllCommandHandlerNames => InertiaServices.GetServiceOrThrow<ReflectionLoaderService>().GetAllCommandHandlerNames();

        public static bool TryHandle(string commandLine)
        {
            return TryHandle(commandLine, null, null);
        }
        public static bool TryHandle(string commandLine, ILogger? logger, object? state)
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

            return TryHandle(commandName.Value, arguments, logger, state);
        }
        public static bool TryHandle(string commandName, string[] args)
        {
            return TryHandle(commandName, null, null, args);
        }
        public static bool TryHandle(string commandName, string[] args, ILogger? logger, object? state)
        {
            if (args.Length == 0) return false;

            var cmd = InertiaServices.GetServiceOrThrow<ReflectionLoaderService>().CreateCommandHandler(commandName, logger ?? NullLogger.Instance, state);
            if (cmd is null) return false;

            cmd.Handle(args);

            return true;
        }
    }
}