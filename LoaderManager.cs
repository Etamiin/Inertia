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
        private static List<IInertiaPlugin> _plugins;

        private static bool _commandsLoaded => Commands != null;
        private static bool _pluginLoaded => _plugins != null;

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

        /// <summary>
        ///
        /// </summary>
        public static void LoadPlugins()
        {
            if (_pluginLoaded || !Directory.Exists("Plugins"))
                return;

            _plugins = new List<IInertiaPlugin>();
            
            var files = IOHelper.GetFilesFromDirectory("Plugins", false);
            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                if (fileInfo.Extension != ".dll")
                    continue;

                var pluginAssembly = Assembly.Load(File.ReadAllBytes(file));

                try
                {
                    foreach (var type in pluginAssembly.GetExportedTypes())
                    {
                        if (type.GetInterface(nameof(IInertiaPlugin)) == null)
                            continue;

                        var instance = (IInertiaPlugin)Activator.CreateInstance(type);
                        instance.OnInitialize();

                        _plugins.Add(instance);
                    }
                }
                catch { }
            }

            foreach (var plugin in _plugins)
                plugin.OnExecute();
        }

        /// <summary>
        /// Returns the specified <see cref="IInertiaPlugin"/> loaded instance.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetPlugin<T>() where T : IInertiaPlugin
        {
            var plugin = _plugins.Find((p) => p.GetType() == typeof(T));
            return plugin != null ? (T)plugin : default(T);
        }
    }
}