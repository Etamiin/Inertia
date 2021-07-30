using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;

namespace Inertia
{
    /// <summary>
    /// 
    /// </summary>
    public static partial class LoaderManager
    {
        internal static Dictionary<string, TextCommand> Commands;

        private static bool _commandsLoaded => Commands != null;

        internal static void DefaultLoadCommands()
        {
            if (_commandsLoaded)
                return;

            Commands = new Dictionary<string, TextCommand>();

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
                            if (Commands.ContainsKey(inertiaCommand.Name))
                                continue;

                            Commands.Add(inertiaCommand.Name, inertiaCommand);
                        }
                    }
                }
            }
        }
    }
}