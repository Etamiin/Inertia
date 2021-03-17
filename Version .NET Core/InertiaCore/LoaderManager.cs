using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Inertia.Internal;
using Inertia.Network;

namespace Inertia
{
    /// <summary>
    /// 
    /// </summary>
    public static class LoaderManager
    {
        internal static Dictionary<string, TextCommand> Commands;
        internal static Dictionary<uint, Type> NetworkMessageTypes;
        internal static Dictionary<Type, NetworkMessageHookerRefs> NetworkMessageHookers;
        private static List<IInertiaPlugin> _plugins;

        private static bool _commandsLoaded => Commands != null;
        private static bool _networkLoaded => NetworkMessageTypes != null;

        internal static void LoadIfNotLoaded()
        {
            if (!_commandsLoaded)
                LoadCommands();
            if (!_networkLoaded)
                LoadNetworkMessages();
            ReloadPlugins(false);
        }

        /// <summary>
        /// Reload all the plugins in memory
        /// </summary>
        public static void ReloadPlugins(bool force = true)
        {
            if (!force && _plugins != null)
                return;
            
            if (_plugins == null)
                _plugins = new List<IInertiaPlugin>();
            else
                _plugins.Clear();

            var files = InertiaIO.GetFilesPathFromDirectory("Plugins", false);
            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                if (fileInfo.Extension != ".dll")
                    continue;

                var pluginAssembly = Assembly.Load(System.IO.File.ReadAllBytes(file));

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
        /// Return the specified <see cref="IInertiaPlugin"/> loaded instance
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetPlugin<T>() where T : IInertiaPlugin
        {
            var plugin = _plugins.Find((p) => p.GetType() == typeof(T));
            return plugin != null ? (T)plugin : default;
        }

        private static void LoadCommands()
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
                            if (Commands.ContainsKey(inertiaCommand.Name))
                                continue;
                            Commands.Add(inertiaCommand.Name, inertiaCommand);
                        }
                    }
                }
            }
        }
        private static void LoadNetworkMessages()
        {
            NetworkMessageTypes = new Dictionary<uint, Type>();
            NetworkMessageHookers = new Dictionary<Type, NetworkMessageHookerRefs>();

            var assemblys = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblys)
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (type.IsClass && type.IsSubclassOf(typeof(NetworkMessage)))
                    {
                        if (type.IsAbstract)
                            continue;

                        var packet = NetworkProtocol.CreateInstance(type);
                        if (NetworkMessageTypes.ContainsKey(packet.Id))
                            continue;

                        NetworkMessageTypes.Add(packet.Id, type);
                    }

                    var sMethods = type.GetMethods(BindingFlags.Static | BindingFlags.Public);
                    if (sMethods.Length == 0)
                        continue;

                    foreach (var smethod in sMethods)
                    {
                        var ps = smethod.GetParameters();
                        if (ps.Length < 2)
                            continue;

                        if (ps[0].ParameterType.IsSubclassOf(typeof(NetworkMessage)) &&
                           (ps[1].ParameterType.IsSubclassOf(typeof(NetClient)) || ps[1].ParameterType == typeof(NetTcpConnection) || ps[1].ParameterType == typeof(NetUdpConnection)))
                        {
                            var msgType = ps[0].ParameterType;

                            if (!NetworkMessageHookers.ContainsKey(msgType))
                                NetworkMessageHookers.Add(msgType, new NetworkMessageHookerRefs());

                            NetworkMessageHookers[msgType].RegisterRef(smethod, ps[1].ParameterType);
                        }
                    }
                }
            }
        }
    }
}
