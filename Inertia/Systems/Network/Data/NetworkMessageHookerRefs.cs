using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Inertia.Network;

namespace Inertia.Internal
{
    internal class NetworkMessageHookerRefs
    {
        public event BasicAction<NetworkMessage, NetTcpClient> TcpClientHook = (msg, cl) => { };
        public event BasicAction<NetworkMessage, NetUdpClient> UdpClientHook = (msg, cl) => { };
        public event BasicAction<NetworkMessage, NetTcpConnection> TcpConnectionHook = (msg, conn) => { };
        public event BasicAction<NetworkMessage, NetUdpConnection> UdpConnectionHook = (msg, conn) => { };
    
        public void RegisterRef(MethodInfo method, Type networkParamType)
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

        public void CallRef(NetworkMessage message, NetTcpClient client)
        {
            TcpClientHook(message, client);
        }
        public void CallRef(NetworkMessage message, NetUdpClient client)
        {
            UdpClientHook(message, client);
        }
        public void CallRef(NetworkMessage message, NetTcpConnection connection)
        {
            TcpConnectionHook(message, connection);
        }
        public void CallRef(NetworkMessage message, NetUdpConnection connection)
        {
            UdpConnectionHook(message, connection);
        }
    }
}
