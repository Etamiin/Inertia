using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Commands
{
    public class CommandManager
    {
        #region GetInstance

        public static CommandManager GetInstance()
        {
            if (Instance == null)
                Instance = new CommandManager();

            return Instance;
        }
        private static CommandManager Instance;

        #endregion

        #region Private variables

        private readonly Dictionary<string, InertiaCommand> Commands;

        #endregion

        #region Constructors

        internal CommandManager()
        {
            Commands = new Dictionary<string, InertiaCommand>();
            LoadCommands();
        }

        #endregion

        public InertiaCommand[] GetCommands()
        {
            var cmds = new InertiaCommand[0];
            lock (Commands)
                cmds = Commands.Values.ToArray();

            return cmds;
        }
        public InertiaCommand GetCommand(string commandName)
        {
            Commands.TryGetValue(commandName, out InertiaCommand command);
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
                        if (type.IsSubclassOf(typeof(InertiaCommand)) && !type.IsAbstract)
                        {
                            var inertiaCommand = (InertiaCommand)type.GetConstructor(Type.EmptyTypes).Invoke(new object[] { });
                            if (Commands.ContainsKey(inertiaCommand.Name))
                                continue;
                            Commands.Add(inertiaCommand.Name, inertiaCommand);
                        }
                    }
                }
            }
        }

        public static bool ExecuteCommandLine(string commandLine)
        {
            var args = commandLine.Split(' ');
            var others = new string[args.Length - 1];
            Array.Copy(args, 1, others, 0, others.Length);

            return ExecuteCommand(args[0], others);
        }
        public static bool ExecuteCommand(string name, params string[] arguments)
        {
            var command = GetInstance().GetCommand(name);
            if (command == null)
                return false;

            command.Execute(new ArgumentsCollection(arguments));
            return true;
        }
    }
}
