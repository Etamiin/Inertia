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

            return command;
        }

        /// <summary>
        /// Executes a command line and returns the associated <see cref="TextCommandArgs"/> instance.
        /// </summary>
        /// <param name="commandLine"></param>
        /// <param name="dataCollection">The list of objects to be associated with the command</param>
        /// <returns></returns>
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
                command.Execute(new TextCommandArgs(commandLine, commandName, arguments, dataCollection));
                return true;
            }

            return false;
        }
    }
}
