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
    ///
    /// </summary>
    public class NetworkMessageCaller
    {
        private event BasicAction<NetworkMessage, TcpClientEntity> OnTcpClient = (msg, cl) => { };
        private event BasicAction<NetworkMessage, UdpClientEntity> OnUdpClient = (msg, cl) => { };
        private event BasicAction<NetworkMessage, TcpConnectionEntity> OnTcpConnection = (msg, conn) => { };
        private event BasicAction<NetworkMessage, UdpConnectionEntity> OnUdpConnection = (msg, conn) => { };

        internal NetworkMessageCaller()
        {
        }

        internal void RegisterReference(MethodInfo method, Type networkParamType)
        {
            if (networkParamType == typeof(TcpClientEntity))
                OnTcpClient += (msg, client) => method.Invoke(null, new object[] { msg, client });
            else if (networkParamType == typeof(UdpClientEntity))
                OnUdpClient += (msg, client) => method.Invoke(null, new object[] { msg, client });
            else if (networkParamType == typeof(TcpConnectionEntity))
                OnTcpConnection += (msg, conn) => method.Invoke(null, new object[] { msg, conn });
            else if (networkParamType == typeof(UdpConnectionEntity))
                OnUdpConnection += (msg, conn) => method.Invoke(null, new object[] { msg, conn });
        }

        /// <summary>
        /// Attempts to execute the specified NetworkMessage.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="client"></param>
        public void TryCall(NetworkMessage message, TcpClientEntity client)
        {
            OnTcpClient(message, client);
        }
        /// <summary>
        /// Attempts to execute the specified NetworkMessage.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="client"></param>
        public void TryCall(NetworkMessage message, UdpClientEntity client)
        {
            OnUdpClient(message, client);
        }
        /// <summary>
        /// Attempts to execute the specified NetworkMessage.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="connection"></param>
        public void TryCall(NetworkMessage message, TcpConnectionEntity connection)
        {
            OnTcpConnection(message, connection);
        }
        /// <summary>
        /// Attempts to execute the specified NetworkMessage.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="connection"></param>
        public void TryCall(NetworkMessage message, UdpConnectionEntity connection)
        {
            OnUdpConnection(message, connection);
        }
    }
}
