using System;
using System.Collections.Generic;
using System.Linq;

namespace Inertia
{
    public static class CommandHooker
    {
        internal static Dictionary<string, TextCommand> _commands;

        static CommandHooker()
        {
            _commands = new Dictionary<string, TextCommand>();

            var assemblys = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblys)
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (type.IsClass)
                    {
                        if (type.IsSubclassOf(typeof(TextCommand)) && !type.IsAbstract)
                        {
                            var inertiaCommand = (TextCommand)type.GetConstructor(Type.EmptyTypes).Invoke(new object[] { });
                            if (!_commands.ContainsKey(inertiaCommand.Name))
                            {
                                _commands.Add(inertiaCommand.Name, inertiaCommand);
                            }
                        }
                    }
                }
            }
        }

        public static TextCommand[] GetAllCommands()
        {
            lock (_commands)
            {
                return _commands.Values.ToArray();
            }
        }
        public static TextCommand GetCommandByName(string commandName)
        {
            _commands.TryGetValue(commandName, out TextCommand command);

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
