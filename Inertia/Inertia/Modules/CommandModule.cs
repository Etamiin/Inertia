using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia
{
    public class CommandModule
    {
        private static CommandModule _instance;

        public static CommandModule Module
        {
            get
            {
                if (_instance == null) {
                    _instance = new CommandModule();
                }
                return _instance;
            }
        }

        private Dictionary<string, CommandBase> _commands;

        internal CommandModule()
        {
            _commands = new Dictionary<string, CommandBase>();
            LoadCommands();
        }

        public static CommandBase[] GetCommands()
        {
            var cmds = new CommandBase[0];
            lock (Module._commands)
                cmds = Module._commands.Values.ToArray();

            return cmds;
        }
        public static CommandBase GetCommand(string commandName)
        {
            Module._commands.TryGetValue(commandName, out CommandBase command);
            return command;
        }

        private void LoadCommands()
        {
            var Assemblys = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in Assemblys)
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (type.IsClass)
                    {
                        if (type.IsSubclassOf(typeof(CommandBase)) && !type.IsAbstract)
                        {
                            var inertiaCommand = (CommandBase)type.GetConstructor(Type.EmptyTypes).Invoke(new object[] { });
                            if (_commands.ContainsKey(inertiaCommand.Name))
                            {
                                Logger.Error($"command [{ inertiaCommand.Name }] already exist");
                                continue;
                            }
                            _commands.Add(inertiaCommand.Name, inertiaCommand);
                        }
                    }
                }
            }
        }

        public static void ExecuteCommandLine(string commandLine)
        {
            var args = commandLine.Split(' ');
            var others = new string[args.Length - 1];
            Array.Copy(args, 1, others, 0, others.Length);

            ExecuteCommand(args[0], others);
        }
        public static void ExecuteCommand(string commandName, string[] commandArguments)
        {
            if (Module._commands.ContainsKey(commandName))
            {
                var inertiaCommand = Module._commands[commandName];
                inertiaCommand.Execute(new CommandArguments(commandArguments));
            }
            else
                Logger.Error($"Unknown command '{ commandName }'");
        }

    }
}
