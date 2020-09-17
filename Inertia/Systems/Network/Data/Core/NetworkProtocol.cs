using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Inertia.Internal;

namespace Inertia.Network
{
    /// <summary>
    /// Represents the class for writing and reading network data
    /// </summary>
    public abstract class NetworkProtocol
    {
        #region Static

        internal static Dictionary<uint, Type> MessageTypes;
        internal static Dictionary<Type, NetworkMessageHookerRefs> MessageHookers;
        internal static NetworkProtocol Protocol { get; private set; } = DefaultNetworkProtocol.Instance;
        
        /// <summary>
        /// Set a custom protocol instance to be used by the system
        /// </summary>
        /// <param name="protocol"><see cref="NetworkProtocol"/> instance to use</param>
        public static void SetProtocol(NetworkProtocol protocol)
        {
            Protocol = protocol;
        }
        /// <summary>
        /// Create an instance of the specified Type if it's an <see cref="NetworkMessage"/>
        /// </summary>
        /// <returns></returns>
        public static NetworkMessage CreateInstance<T>() where T : NetworkMessage
        {
            return CreateInstance(typeof(T));
        }
        /// <summary>
        /// Create an instance of the specified Type if it's an <see cref="NetworkMessage"/>
        /// </summary>
        /// <param name="packetType"></param>
        /// <returns></returns>
        public static NetworkMessage CreateInstance(Type packetType)
        {
            if (packetType.IsAbstract || !packetType.IsSubclassOf(typeof(NetworkMessage)))
                return null;

            var constr = packetType.GetConstructors()[0];
            var parameters = constr.GetParameters();
            var objs = new object[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
                objs[i] = null;

            return (NetworkMessage)constr.Invoke(objs);
        }
        /// <summary>
        /// Create an instance of a <see cref="NetworkMessage"/> based on his Id
        /// </summary>
        /// <param name="packetId">Id of the target <see cref="NetworkMessage"/></param>
        /// <returns></returns>
        public static NetworkMessage CreateInstance(uint packetId)
        {
            if (!MessageTypes.ContainsKey(packetId))
                return null;

            return CreateInstance(MessageTypes[packetId]);
        }

        /// <summary>
        /// Execute a <see cref="NetworkMessageHooker"/> if exist
        /// </summary>
        /// <param name="message"></param>
        /// <param name="networkObj"></param>
        public static bool ExecuteHooker(NetworkMessage message, object networkObj)
        {
            if (MessageHookers.TryGetValue(message.GetType(), out NetworkMessageHookerRefs refs))
            {
                try
                {
                    if (networkObj is NetTcpClient)
                        refs.CallRef(message, networkObj as NetTcpClient);
                    else if (networkObj is NetUdpClient)
                        refs.CallRef(message, networkObj as NetUdpClient);
                    else if (networkObj is NetTcpConnection)
                        refs.CallRef(message, networkObj as NetTcpConnection);
                    else if (networkObj is NetUdpConnection)
                        refs.CallRef(message, networkObj as NetUdpConnection);
                }
                catch { }
            }

            return false;
        }

        #endregion
        
        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        public NetworkProtocol()
        {
            MessageTypes = new Dictionary<uint, Type>();
            MessageHookers = new Dictionary<Type, NetworkMessageHookerRefs>();

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

                        var packet = CreateInstance(type);
                        if (MessageTypes.ContainsKey(packet.Id))
                            continue;

                        MessageTypes.Add(packet.Id, type);
                    }

                    var sMethods = type.GetMethods(BindingFlags.Static | BindingFlags.Public);
                    if (sMethods.Length == 0)
                        continue;

                    foreach (var smethod in sMethods)
                    {
                        var attr = smethod.GetCustomAttribute<NetworkMessageHooker>();
                        if (attr != null)
                        {
                            var ps = smethod.GetParameters();
                            if (ps.Length < 2)
                                continue;

                            if (ps[0].ParameterType.IsSubclassOf(typeof(NetworkMessage)) && 
                               (ps[1].ParameterType.IsSubclassOf(typeof(NetClient)) || ps[1].ParameterType == typeof(NetTcpConnection) || ps[1].ParameterType == typeof(NetUdpConnection)))
                            {
                                if (!MessageHookers.ContainsKey(attr.MessageType))
                                    MessageHookers.Add(attr.MessageType, new NetworkMessageHookerRefs());

                                MessageHookers[attr.MessageType].RegisterRef(smethod, ps[1].ParameterType);
                            }
                        }
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Happens when parsing a <see cref="NetworkMessage"/> instance
        /// </summary>
        /// <param name="packet">Packet to parse</param>
        /// <returns>Parsed packet to byte array</returns>
        public virtual byte[] OnParsePacket(NetworkMessage packet)
        {
            return DefaultNetworkProtocol.Instance.OnParsePacket(packet);
        }

        /// <summary>
        /// Happens when receiving data from a <see cref="NetTcpClient"/>
        /// </summary>
        /// <param name="client">The connection that received the data</param>
        /// <param name="reader">The data in a <see cref="BasicReader"/> instance</param>
        public virtual void OnReceiveData(NetTcpClient client, BasicReader reader)
        {
            DefaultNetworkProtocol.Instance.OnReceiveData(client, reader);
        }
        /// <summary>
        /// Happens when receiving data from a <see cref="NetUdpClient"/>
        /// </summary>
        /// <param name="client">The connection that received the data</param>
        /// <param name="reader">The data in a <see cref="BasicReader"/> instance</param>
        public virtual void OnReceiveData(NetUdpClient client, BasicReader reader)
        {
            DefaultNetworkProtocol.Instance.OnReceiveData(client, reader);
        }
        /// <summary>
        /// Happens when receiving data from a <see cref="NetTcpConnection"/>
        /// </summary>
        /// <param name="connection">The connection that received the data</param>
        /// <param name="reader">The data in a <see cref="BasicReader"/> instance</param>
        public virtual void OnReceiveData(NetTcpConnection connection, BasicReader reader)
        {
            DefaultNetworkProtocol.Instance.OnReceiveData(connection, reader);
        }
        /// <summary>
        /// Happens when receiving data from a <see cref="NetUdpConnection"/>
        /// </summary>
        /// <param name="connection">The connection that received the data</param>
        /// <param name="reader">The data in a <see cref="BasicReader"/> instance</param>
        public virtual void OnReceiveData(NetUdpConnection connection, BasicReader reader)
        {
            DefaultNetworkProtocol.Instance.OnReceiveData(connection, reader);
        }
    
    }
}
