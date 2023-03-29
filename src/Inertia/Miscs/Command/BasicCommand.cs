using System.Collections.Generic;
using System.Text;

namespace Inertia
{
    public abstract class BasicCommand
    {
        internal static void PreExecute(BasicCommand command, string[] arguments, object? state, bool containsQuotes)
        {
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

            command.OnExecute(new CommandArguments(arguments, state));
        }
        
        public abstract string Name { get; }
        
        public abstract void OnExecute(CommandArguments arguments);
    }
}