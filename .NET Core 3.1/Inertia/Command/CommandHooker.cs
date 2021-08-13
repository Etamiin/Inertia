using System;
using System.Linq;

namespace Inertia
{
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

            var cmds = new TextCommand[0];
            lock (LoaderManager.Commands)
                cmds = LoaderManager.Commands.Values.ToArray();

            return cmds;
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

            return command;
        }

        /// <summary>
        /// Executes a command line and returns the associated <see cref="TextCommandArgs"/> instance.
        /// </summary>
        /// <param name="commandLine"></param>
        /// <param name="dataCollection">The list of objects to be associated with the command</param>
        /// <returns></returns>
        public static TextCommandArgs TryExecuteTextCommand(string commandLine, params object[] dataCollection)
        {
            var args = commandLine.Split(' ');
            var others = new string[args.Length - 1];
            Array.Copy(args, 1, others, 0, others.Length);

            return TryExecuteCommandByName(commandLine, args[0], dataCollection, others);
        }
        /// <summary>
        /// Executes a <see cref="TextCommand"/> associated with the specified name.
        /// </summary>
        /// <param name="commandName"></param>
        /// <param name="dataCollection">The list of objects to be associated with the command</param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public static TextCommandArgs TryExecuteCommandByName(string commandName, object[] dataCollection, params string[] arguments)
        {
            return TryExecuteCommandByName(string.Empty, commandName, dataCollection, arguments);
        }

        private static TextCommandArgs TryExecuteCommandByName(string commandLine, string commandName, object[] dataCollection = null, params string[] arguments)
        {
            var command = GetCommandByName(commandName);
            if (command == null) return null;

            var args = new TextCommandArgs(commandLine, commandName, arguments, dataCollection);
            command.Execute(args);

            return args;
        }
    }
}
