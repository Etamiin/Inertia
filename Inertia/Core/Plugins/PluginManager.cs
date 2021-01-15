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
    public static class PluginManager
    {
        internal static bool PluginsLoaded { get; private set; }
        private static List<Assembly> m_pluginAssemblys;
        private static List<InertiaPlugin> m_plugins;

        /// <summary>
        /// Reload all the plugins in memory
        /// </summary>
        public static void ReloadPlugins()
        {
            PluginsLoaded = true;

            if (m_pluginAssemblys == null)
                m_pluginAssemblys = new List<Assembly>();
            else
                m_pluginAssemblys.Clear();
            
            if (m_plugins == null)
                m_plugins = new List<InertiaPlugin>();
            else
                m_plugins.Clear();

            var pluginFolder = "Plugins";
            if (!Directory.Exists(pluginFolder))
            {
                pluginFolder = pluginFolder.ToLower();
                if (!Directory.Exists(pluginFolder))
                    pluginFolder = pluginFolder.ToUpper();
            }

            var files = InertiaIO.GetFilesPathFromDirectory(pluginFolder, false);
            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                if (fileInfo.Extension != ".dll")
                    continue;

                var pluginAssembly = Assembly.LoadFile(file);

                try
                {
                    foreach (var type in pluginAssembly.GetExportedTypes())
                    {
                        if (!type.IsSubclassOf(typeof(InertiaPlugin)))
                            continue;

                        var instance = (InertiaPlugin)Activator.CreateInstance(type);
                        instance.OnInitialize();

                        m_plugins.Add(instance);
                    }

                    m_pluginAssemblys.Add(pluginAssembly);
                }
                catch { }
            }

            foreach (var plugin in m_plugins)
                plugin.OnExecute();
        }

        /// <summary>
        /// Return the specified Type of <see cref="InertiaPlugin"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetPlugin<T>() where T : InertiaPlugin
        {
            var plugin = m_plugins.Find((p) => p.GetType() == typeof(T));
            return plugin != null ? (T)plugin : null;
        }

        internal static Dictionary<string, TextCommand> LoadCommands()
        {
            if (!PluginsLoaded)
                ReloadPlugins();

            var commands = new Dictionary<string, TextCommand>();
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
                            if (commands.ContainsKey(inertiaCommand.Name))
                                continue;
                            commands.Add(inertiaCommand.Name, inertiaCommand);
                        }
                    }
                }
            }

            return commands;
        }
        internal static void LoadNetworkMessages(out Dictionary<uint, Type> messageTypes, out Dictionary<Type, NetworkMessageHookerRefs> messageHookers)
        {
            if (!PluginsLoaded)
                ReloadPlugins();

            messageTypes = new Dictionary<uint, Type>();
            messageHookers = new Dictionary<Type, NetworkMessageHookerRefs>();

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
                        if (messageTypes.ContainsKey(packet.Id))
                            continue;

                        messageTypes.Add(packet.Id, type);
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

                            if (!messageHookers.ContainsKey(msgType))
                                messageHookers.Add(msgType, new NetworkMessageHookerRefs());

                            messageHookers[msgType].RegisterRef(smethod, ps[1].ParameterType);
                        }
                    }
                }
            }
        }
    }
}
