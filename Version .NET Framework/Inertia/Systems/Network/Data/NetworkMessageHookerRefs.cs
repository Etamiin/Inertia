using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Network
{
    /// <summary>
    /// MessageHookers references
    /// </summary>
    public class NetworkMessageHookerRefs
    {
        internal event BasicAction<NetworkMessage, NetTcpClient> TcpClientHook = (msg, cl) => { };
        internal event BasicAction<NetworkMessage, NetUdpClient> UdpClientHook = (msg, cl) => { };
        internal event BasicAction<NetworkMessage, NetTcpConnection> TcpConnectionHook = (msg, conn) => { };
        internal event BasicAction<NetworkMessage, NetUdpConnection> UdpConnectionHook = (msg, conn) => { };

        #region Constructors

        internal NetworkMessageHookerRefs()
        {
        }

        #endregion

        internal void RegisterRef(MethodInfo method, Type networkParamType)
        {
            if (networkParamType == typeof(NetTcpClient))
                TcpClientHook += (msg, client) => method.Invoke(null, new object[] { msg, client });
            else if (networkParamType == typeof(NetUdpClient))
                UdpClientHook += (msg, client) => method.Invoke(null, new object[] { msg, client });
            else if (networkParamType == typeof(NetTcpConnection))
                TcpConnectionHook += (msg, conn) => method.Invoke(null, new object[] { msg, conn });
            else if (networkParamType == typeof(NetUdpConnection))
                UdpConnectionHook += (msg, conn) => method.Invoke(null, new object[] { msg, conn });
        }

        /// <summary>
        /// Invoke a MessageHooker with the specified parameters
        /// </summary>
        /// <param name="message"></param>
        /// <param name="client"></param>
        public void CallHookerRef(NetworkMessage message, NetTcpClient client)
        {
            TcpClientHook(message, client);
        }
        /// <summary>
        /// Invoke a MessageHooker with the specified parameters
        /// </summary>
        /// <param name="message"></param>
        /// <param name="client"></param>
        public void CallHookerRef(NetworkMessage message, NetUdpClient client)
        {
            UdpClientHook(message, client);
        }
        /// <summary>
        /// Invoke a MessageHooker with the specified parameters
        /// </summary>
        /// <param name="message"></param>
        /// <param name="connection"></param>
        public void CallHookerRef(NetworkMessage message, NetTcpConnection connection)
        {
            TcpConnectionHook(message, connection);
        }
        /// <summary>
        /// Invoke a MessageHooker with the specified parameters
        /// </summary>
        /// <param name="message"></param>
        /// <param name="connection"></param>
        public void CallHookerRef(NetworkMessage message, NetUdpConnection connection)
        {
            UdpConnectionHook(message, connection);
        }
    }
}
