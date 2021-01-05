using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia
{
    /// <summary>
    /// Represent the class that will manage the command reception and execution
    /// </summary>
    public static class CommandHooker
    {
        #region Private variables

        private static Dictionary<string, TextCommand> m_commands;
        private static bool Initializated
        {
            get
            {
                return m_commands != null;
            }
        }

        #endregion

        private static void Initialize()
        {
            if (Initializated)
                return;

            m_commands = PluginManager.LoadCommands();
        }

        /// <summary>
        /// Return all existing <see cref="TextCommand"/> instancied
        /// </summary>
        /// <returns>An array of <see cref="TextCommand"/> representing the existing <see cref="TextCommand"/></returns>
        public static TextCommand[] GetAllCommands()
        {
            if (!Initializated)
                Initialize();

            var cmds = new TextCommand[0];
            lock (m_commands)
                cmds = m_commands.Values.ToArray();

            return cmds;
        }
        /// <summary>
        /// Return the <see cref="TextCommand"/> associated to the specified name 
        /// </summary>
        /// <param name="commandName">The name of the command</param>
        /// <returns>The <see cref="TextCommand"/> associated or null if none</returns>
        public static TextCommand GetCommandByName(string commandName)
        {
            if (!Initializated)
                Initialize();

            m_commands.TryGetValue(commandName, out TextCommand command);
            return command;
        }

        /// <summary>
        /// Parse a string command line to a <see cref="TextCommand"/> and execute it with facultative data
        /// </summary>
        /// <param name="commandLine">The string line to parse</param>
        /// <param name="dataCollection">Facultative data objects</param>
        /// <returns>Return true if the command was executed</returns>
        public static TextCommandArgs ExecuteTextCommand(string commandLine, params object[] dataCollection)
        {
            var args = commandLine.Split(' ');
            var others = new string[args.Length - 1];
            Array.Copy(args, 1, others, 0, others.Length);

            return ExecuteCommandByName(args[0], dataCollection, others);
        }
        /// <summary>
        /// Get the associated <see cref="TextCommand"/> based on the name and execute it with the specified arguments
        /// </summary>
        /// <param name="commandName">The command anme</param>
        /// <param name="dataCollection">Facultative data objects</param>
        /// <param name="arguments">Facultative string arguments</param>
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
