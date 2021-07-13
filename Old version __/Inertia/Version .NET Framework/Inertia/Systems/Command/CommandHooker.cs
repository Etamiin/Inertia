using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia
{
    /// <summary>
    /// Manage all commands system
    /// </summary>
    public static class CommandHooker
    {
        /// <summary>
        /// Returns all existing <see cref="TextCommand"/> loaded
        /// </summary>
        /// <returns></returns>
        public static TextCommand[] GetAllCommands()
        {
            LoaderManager.ReloadAll();

            var cmds = new TextCommand[0];
            lock (LoaderManager.Commands)
                cmds = LoaderManager.Commands.Values.ToArray();

            return cmds;
        }
        /// <summary>
        /// Returns the <see cref="TextCommand"/> associated to the specified name 
        /// </summary>
        /// <param name="commandName">The name of the command</param>
        /// <returns></returns>
        public static TextCommand GetCommandByName(string commandName)
        {
            LoaderManager.ReloadAll();
            LoaderManager.Commands.TryGetValue(commandName, out TextCommand command);

            return command;
        }

        /// <summary>
        /// Parse a string command line to a <see cref="TextCommand"/> and execute it
        /// </summary>
        /// <param name="commandLine">The string line to parse</param>
        /// <param name="dataCollection">Data objects</param>
        /// <returns>Return <see cref="TextCommandArgs"/> instance created</returns>
        public static TextCommandArgs ExecuteTextCommand(string commandLine, params object[] dataCollection)
        {
            var args = commandLine.Split(' ');
            var others = new string[args.Length - 1];
            Array.Copy(args, 1, others, 0, others.Length);

            return ExecuteCommandByName(args[0], dataCollection, others);
        }
        /// <summary>
        /// Get the associated <see cref="TextCommand"/> based on the name and execute it
        /// </summary>
        /// <param name="commandName">The command name</param>
        /// <param name="dataCollection">Data objects</param>
        /// <param name="arguments"><see cref="string"/> arguments</param>
        /// <returns></returns>
        public static TextCommandArgs ExecuteCommandByName(string commandName, object[] dataCollection = null, params string[] arguments)
        {
            var command = GetCommandByName(commandName);
            if (command == null)
                return null;

            var args = new TextCommandArgs(commandName, arguments, dataCollection);
            command.Execute(args);

            return args;
        }
    }
}
