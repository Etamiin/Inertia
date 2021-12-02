using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Network
{
    public sealed class NetworkMessageCaller
    {
<<<<<<< HEAD
        private event BasicAction<NetworkMessage, TcpClientEntity> OnTcpClient;
        private event BasicAction<NetworkMessage, UdpClientEntity> OnUdpClient;
        private event BasicAction<NetworkMessage, TcpConnectionEntity> OnTcpConnection;
        private event BasicAction<NetworkMessage, UdpConnectionEntity> OnUdpConnection;

        internal NetworkMessageCaller()
        {
        }

        internal void RegisterReference(MethodInfo method, Type networkParamType)
        {
            if (networkParamType == typeof(TcpClientEntity))
            {
                OnTcpClient += (msg, client) => method.Invoke(null, new object[] { msg, client });
            }
            else if (networkParamType == typeof(UdpClientEntity))
            {
                OnUdpClient += (msg, client) => method.Invoke(null, new object[] { msg, client });
            }
            else if (networkParamType == typeof(TcpConnectionEntity))
            {
                OnTcpConnection += (msg, conn) => method.Invoke(null, new object[] { msg, conn });
            }
            else if (networkParamType == typeof(UdpConnectionEntity))
            {
                OnUdpConnection += (msg, conn) => method.Invoke(null, new object[] { msg, conn });
            }
        }

        /// <summary>
        /// Attempts to execute the specified NetworkMessage.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="client"></param>
        public void TryCall(NetworkMessage message, TcpClientEntity client)
        {
            OnTcpClient?.Invoke(message, client);
        }
        /// <summary>
        /// Attempts to execute the specified NetworkMessage.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="client"></param>
        public void TryCall(NetworkMessage message, UdpClientEntity client)
        {
            OnUdpClient?.Invoke(message, client);
        }
        /// <summary>
        /// Attempts to execute the specified NetworkMessage.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="connection"></param>
        public void TryCall(NetworkMessage message, TcpConnectionEntity connection)
        {
            OnTcpConnection?.Invoke(message, connection);
        }
        /// <summary>
        /// Attempts to execute the specified NetworkMessage.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="connection"></param>
        public void TryCall(NetworkMessage message, UdpConnectionEntity connection)
        {
            OnUdpConnection?.Invoke(message, connection);
=======
        private Dictionary<Type, MethodInfo> _references;

        internal NetworkMessageCaller()
        {
            _references = new Dictionary<Type, MethodInfo>();
        }

        internal void RegisterReference(Type messageType, MethodInfo method)
        {
            if (!_references.ContainsKey(messageType))
            {
                _references.Add(messageType, method);
            }
            else
            {
                _references[messageType] = method;
            }
        }

        public void CallReferences(NetworkMessage message, object receiver)
        {
            foreach (var reference in _references)
            {
                reference.Value.Invoke(null, new object[] { message, receiver });
            }
>>>>>>> premaster
        }
    }
}
