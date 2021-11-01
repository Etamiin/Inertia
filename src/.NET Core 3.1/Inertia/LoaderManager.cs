using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Linq;
using Inertia.Runtime;

namespace Inertia
{
    public static class LoaderManager
    {
        private static List<IInertiaPlugin> _plugins;
        private static bool _pluginLoaded => _plugins != null;

        public static void LoadPlugins()
        {
            if (_pluginLoaded || !Directory.Exists("Plugins"))
            {
                return;
            }

            _plugins = new List<IInertiaPlugin>();

            var files = IOHelper.GetFilesFromDirectory("Plugins", false);
            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                if (fileInfo.Extension.Equals(".dll"))
                {
                    var pluginAssembly = Assembly.Load(File.ReadAllBytes(file));

                    foreach (var type in pluginAssembly.GetExportedTypes())
                    {
                        if (type.GetInterface(nameof(IInertiaPlugin)) != null)
                        {
                            var instance = (IInertiaPlugin)Activator.CreateInstance(type);
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
        /// Returns the specified <see cref="IInertiaPlugin"/> loaded instance.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool TryGetPlugin<T>(out T plugin) where T : IInertiaPlugin
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
    }
}