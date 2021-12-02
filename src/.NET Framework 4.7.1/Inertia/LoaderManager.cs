using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Linq;
using Inertia.Runtime;

namespace Inertia
{
<<<<<<< HEAD
<<<<<<< HEAD
    /// <summary>
    /// 
    /// </summary>
    public static class LoaderManager
    {
        internal static Dictionary<string, TextCommand> Commands;

        private static List<IInertiaPlugin> _plugins;
        private static bool _commandsLoaded => Commands != null;
        private static bool _pluginLoaded => _plugins != null;

        /// <summary>
        ///
        /// </summary>
=======
    public static class LoaderManager
    {
        private static List<IInertiaPlugin> _plugins;
        private static bool _pluginLoaded => _plugins != null;

>>>>>>> 9bfc85f6784b254a10c65f104446a83c8b195c40
=======
    [Obsolete]
    public static class LoaderManager
    {
        private static List<IPlugin> _plugins;
        private static bool _pluginLoaded => _plugins != null;

        [Obsolete]
>>>>>>> premaster
        public static void LoadPlugins()
        {
            if (_pluginLoaded || !Directory.Exists("Plugins"))
            {
                return;
            }

<<<<<<< HEAD
            _plugins = new List<IInertiaPlugin>();
=======
            _plugins = new List<IPlugin>();
>>>>>>> premaster

            var files = IOHelper.GetFilesFromDirectory("Plugins", false);
            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                if (fileInfo.Extension.Equals(".dll"))
                {
                    var pluginAssembly = Assembly.Load(File.ReadAllBytes(file));

                    foreach (var type in pluginAssembly.GetExportedTypes())
                    {
<<<<<<< HEAD
                        if (type.GetInterface(nameof(IInertiaPlugin)) != null)
                        {
                            var instance = (IInertiaPlugin)Activator.CreateInstance(type);
=======
                        if (type.GetInterface(nameof(IPlugin)) != null)
                        {
                            var instance = (IPlugin)Activator.CreateInstance(type);
>>>>>>> premaster
                            instance.OnInitialize();

                            _plugins.Add(instance);
                        }
                    }
                }                
            }

            foreach (var plugin in _plugins)
            {
                plugin.OnExecute();
            }
        }

        /// <summary>
<<<<<<< HEAD
        /// Returns the specified <see cref="IInertiaPlugin"/> loaded instance.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool TryGetPlugin<T>(out T plugin) where T : IInertiaPlugin
=======
        /// Returns the specified <see cref="IPlugin"/> loaded instance.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [Obsolete]
        public static bool TryGetPlugin<T>(out T plugin) where T : IPlugin
>>>>>>> premaster
        {
            var pl = _plugins.FirstOrDefault((p) => p.GetType() == typeof(T));
            if (pl != null)
            {
                plugin = (T)pl;
                return true;
            }

            plugin = default;
            return false;
        }
<<<<<<< HEAD
<<<<<<< HEAD

        internal static void DefaultLoadCommands()
        {
            if (!_commandsLoaded)
            {
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
                                if (!Commands.ContainsKey(inertiaCommand.Name))
                                {
                                    Commands.Add(inertiaCommand.Name, inertiaCommand);
                                }
                            }
                        }
                    }
                }
            }            
        }
=======
>>>>>>> 9bfc85f6784b254a10c65f104446a83c8b195c40
=======
>>>>>>> premaster
    }
}