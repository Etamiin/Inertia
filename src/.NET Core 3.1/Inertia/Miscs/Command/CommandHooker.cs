using System;
using System.Collections.Generic;
using System.Linq;

namespace Inertia
{
    /// <summary>
    /// Core component for command line management and execution.
    /// </summary>
    public static class CommandHooker
    {
        private static Dictionary<string, BasicCommand> _commands;

        static CommandHooker()
        {
            _commands = new Dictionary<string, BasicCommand>();

            var assemblys = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblys)
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (type.IsClass)
                    {
                        if (type.IsSubclassOf(typeof(BasicCommand)) && !type.IsAbstract)
                        {
                            try
                            {
                                var instance = (BasicCommand)Activator.CreateInstance(type);
                                if (!_commands.ContainsKey(instance.Name))
                                {
                                    _commands.Add(instance.Name, instance);
                                }
                            }
                            catch (Exception ex)
                            {
                                throw new InertiaInitializationException(nameof(CommandHooker), ex);
                            }                            
                        }
                    }
                }
            }
        }

        public static BasicCommand[] GetCommandList()
        {
            lock (_commands)
            {
                return _commands.Values.ToArray();
            }
        }
        public static bool TryGetCommand(string commandName, out BasicCommand command)
        {
            return _commands.TryGetValue(commandName, out command);            
        }

        public static bool TryExecute(string commandLine, params object[] dataCollection)
        {
            var args = commandLine.Split(' ');
            var others = new string[args.Length - 1];
            Array.Copy(args, 1, others, 0, others.Length);

            return TryExecuteByName(args[0], dataCollection, commandLine.Contains('"'), others);
        }
        
        private static bool TryExecuteByName(string commandName, object[] dataCollection, bool containsBlock, params string[] arguments)
        {
            if (TryGetCommand(commandName, out BasicCommand cmd))
            {
                cmd.PreExecute(arguments, dataCollection, containsBlock);
                return true;
            }

            return false;
        }
    }
}
