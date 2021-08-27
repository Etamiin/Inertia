using System;
using System.Collections.Generic;
using System.Reflection;

namespace Inertia.Network
{
    /// <summary>
    /// 
    /// </summary>
    public static partial class LoaderManager
    {
        internal static Dictionary<uint, Type> NetworkMessageTypes;
        internal static Dictionary<Type, NetworkMessageCaller> NetworkMessageHookers;

        private static bool _networkLoaded => NetworkMessageTypes != null;

        internal static void DefaultLoadNetwork()
        {
            if (_networkLoaded)
                return;

            NetworkMessageTypes = new Dictionary<uint, Type>();
            NetworkMessageHookers = new Dictionary<Type, NetworkMessageCaller>();

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

                        var message = NetworkProtocol.CreateMessage(type);
                        if (NetworkMessageTypes.ContainsKey(message.MessageId))
                            continue;

                        NetworkMessageTypes.Add(message.MessageId, type);
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
                           (ps[1].ParameterType.IsSubclassOf(typeof(NetworkClientEntity)) || ps[1].ParameterType == typeof(TcpConnectionEntity) || ps[1].ParameterType == typeof(UdpConnectionEntity)))
                        {
                            var msgType = ps[0].ParameterType;

                            if (!NetworkMessageHookers.ContainsKey(msgType))
                                NetworkMessageHookers.Add(msgType, new NetworkMessageCaller());

                            NetworkMessageHookers[msgType].RegisterReference(smethod, ps[1].ParameterType);
                        }
                    }
                }
            }
        }
    }
}