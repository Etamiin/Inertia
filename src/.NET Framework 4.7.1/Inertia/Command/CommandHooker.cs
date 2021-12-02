using System;
<<<<<<< HEAD
<<<<<<< HEAD
=======
using System.Collections.Generic;
>>>>>>> 9bfc85f6784b254a10c65f104446a83c8b195c40
=======
using System.Collections.Generic;
>>>>>>> premaster
using System.Linq;

namespace Inertia
{
<<<<<<< HEAD
<<<<<<< HEAD
    /// <summary>
    ///
    /// </summary>
    public static class CommandHooker
    {
        /// <summary>
        /// Returns all loaded textual commands.
        /// </summary>
        /// <returns></returns>
        public static TextCommand[] GetAllCommands()
        {
            LoaderManager.DefaultLoadCommands();

            lock (LoaderManager.Commands)
            {
                return LoaderManager.Commands.Values.ToArray();
            }
        }
        /// <summary>
        /// Returns the <see cref="TextCommand"/> associated with the specified name.
        /// </summary>
        /// <param name="commandName"></param>
        /// <returns></returns>
        public static TextCommand GetCommandByName(string commandName)
        {
            LoaderManager.DefaultLoadCommands();
            LoaderManager.Commands.TryGetValue(commandName, out TextCommand command);
=======
    public static class CommandHooker
    {
        internal static Dictionary<string, TextCommand> _commands;

        static CommandHooker()
        {
            _commands = new Dictionary<string, TextCommand>();
=======
    /// <summary>
    /// Core component for command line management and execution.
    /// </summary>
    public static class CommandHooker
    {
        private static Dictionary<string, BasicCommand> _commands;

        static CommandHooker()
        {
            _commands = new Dictionary<string, BasicCommand>();
>>>>>>> premaster

            var assemblys = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblys)
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (type.IsClass)
                    {
<<<<<<< HEAD
                        if (type.IsSubclassOf(typeof(TextCommand)) && !type.IsAbstract)
                        {
                            var inertiaCommand = (TextCommand)type.GetConstructor(Type.EmptyTypes).Invoke(new object[] { });
                            if (!_commands.ContainsKey(inertiaCommand.Name))
                            {
                                _commands.Add(inertiaCommand.Name, inertiaCommand);
                            }
=======
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
>>>>>>> premaster
                        }
                    }
                }
            }
        }

<<<<<<< HEAD
        public static TextCommand[] GetAllCommands()
=======
        public static BasicCommand[] GetCommandList()
>>>>>>> premaster
        {
            lock (_commands)
            {
                return _commands.Values.ToArray();
            }
        }
<<<<<<< HEAD
        public static TextCommand GetCommandByName(string commandName)
        {
            _commands.TryGetValue(commandName, out TextCommand command);
>>>>>>> 9bfc85f6784b254a10c65f104446a83c8b195c40

            return command;
        }

<<<<<<< HEAD
        /// <summary>
        /// Executes a command line and returns the associated <see cref="TextCommandArgs"/> instance.
        /// </summary>
        /// <param name="commandLine"></param>
        /// <param name="dataCollection">The list of objects to be associated with the command</param>
        /// <returns></returns>
=======
>>>>>>> 9bfc85f6784b254a10c65f104446a83c8b195c40
        public static bool TryExecuteCommandLine(string commandLine, params object[] dataCollection)
=======
        public static bool TryGetCommand(string commandName, out BasicCommand command)
        {
            return _commands.TryGetValue(commandName, out command);            
        }

        public static bool TryExecute(string commandLine, params object[] dataCollection)
>>>>>>> premaster
        {
            var args = commandLine.Split(' ');
            var others = new string[args.Length - 1];
            Array.Copy(args, 1, others, 0, others.Length);

<<<<<<< HEAD
            return TryExecuteCommandByName(commandLine, args[0], dataCollection, others);
        }
        
        private static bool TryExecuteCommandByName(string commandLine, string commandName, object[] dataCollection = null, params string[] arguments)
        {
            var command = GetCommandByName(commandName);
            if (command != null)
            {
<<<<<<< HEAD
                command.Execute(new TextCommandArgs(commandLine, commandName, arguments, dataCollection));
=======
                command.OnExecute(new TextCommandArgs(commandLine, arguments, dataCollection));
>>>>>>> 9bfc85f6784b254a10c65f104446a83c8b195c40
=======
            return TryExecuteByName(args[0], dataCollection, commandLine.Contains('"'), others);
        }
        
        private static bool TryExecuteByName(string commandName, object[] dataCollection, bool containsBlock, params string[] arguments)
        {
            if (TryGetCommand(commandName, out BasicCommand cmd))
            {
                cmd.PreExecute(arguments, dataCollection, containsBlock);
>>>>>>> premaster
                return true;
            }

            return false;
        }
    }
}
