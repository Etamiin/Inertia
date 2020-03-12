using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inertia.Commands;

namespace Inertia.Internal
{
    internal class HelpCommand : InertiaCommand
    {
        #region Public variables

        public override string Name => "help";

        #endregion

        public override void Execute(ArgumentsCollection collection)
        {
            if (collection.NextArgument(out string name)) {
                var command = CommandManager.GetInstance().GetCommand(name);
                if (command == null)
                    return;

                InertiaLogger.Log("Help for command({0}): {1}", name, command.GetHelp());
            }
            else
                InertiaLogger.Log(GetHelp());
        }

        public override string GetHelp()
        {
            return
                "Return the command documentation associated to the command" +
                Environment.NewLine +
                "Example of use: 'help myCommandName'";
        }
    }
}
