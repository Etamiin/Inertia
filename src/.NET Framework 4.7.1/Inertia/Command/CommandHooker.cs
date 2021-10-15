using System;
using System.Linq;

namespace Inertia
{
    public static class CommandHooker
    {
        public static TextCommand[] GetAllCommands()
        {
            LoaderManager.DefaultLoadCommands();

            lock (LoaderManager.Commands)
            {
                return LoaderManager.Commands.Values.ToArray();
            }
        }
        public static TextCommand GetCommandByName(string commandName)
        {
            LoaderManager.DefaultLoadCommands();
            LoaderManager.Commands.TryGetValue(commandName, out TextCommand command);

            return command;
        }

        public static bool TryExecuteCommandLine(string commandLine, params object[] dataCollection)
        {
            var args = commandLine.Split(' ');
            var others = new string[args.Length - 1];
            Array.Copy(args, 1, others, 0, others.Length);

            return TryExecuteCommandByName(commandLine, args[0], dataCollection, others);
        }
        
        private static bool TryExecuteCommandByName(string commandLine, string commandName, object[] dataCollection = null, params string[] arguments)
        {
            var command = GetCommandByName(commandName);
            if (command != null)
            {
                command.OnExecute(new TextCommandArgs(commandLine, arguments, dataCollection));
                return true;
            }

            return false;
        }
    }
}
