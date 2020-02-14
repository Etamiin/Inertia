using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Network
{
    public class NetworkServer : IDisposable
    {
        #region Public variables

        public TcpNetworkServer Tcp { get; private set; }
        public UdpNetworkServer Udp { get; private set; }

        #endregion

        #region Constructors

        public NetworkServer(string ip, int tcpPort, int udpPort)
        {
            Tcp = new TcpNetworkServer(ip, tcpPort);
            Udp = new UdpNetworkServer(ip, udpPort);
        }
        public NetworkServer(string tcpIp, int tcpPort, string udpIp, int UdpPort)
        {
            Tcp = new TcpNetworkServer(tcpIp, tcpPort);
            Udp = new UdpNetworkServer(udpIp, UdpPort);
        }

        #endregion

        public void Open()
        {
            Tcp.Open();
            Udp.Open();
        }
        public void Close(NetworkDisconnectReason reason)
        {
            Tcp.Close(reason);
            Udp.Close(reason);
        }

        public void Dispose()
        {
            Tcp?.Dispose();
            Udp?.Dispose();
            Tcp = null;
            Udp = null;
        }
    }
}
