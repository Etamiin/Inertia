using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Network
{
    public class NetworkClient : IDisposable
    {
        #region Public variables

        public TcpNetworkClient Tcp { get; private set; }
        public UdpNetworkClient Udp { get; private set; }

        #endregion

        #region Constructors

        public NetworkClient(string ip, int tcpPort, int udpPort)
        {
            Tcp = new TcpNetworkClient(ip, tcpPort);
            Udp = new UdpNetworkClient(ip, udpPort);
        }
        public NetworkClient(string tcpIp, int tcpPort, string udpip, int udpPort)
        {
            Tcp = new TcpNetworkClient(tcpIp, tcpPort);
            Udp = new UdpNetworkClient(udpip, udpPort);
        }

        #endregion

        public void Connect()
        {
            Tcp.Connect();
            Udp.Connect();
        }
        public void Disconnect(NetworkDisconnectReason reason)
        {
            Tcp.Disconnect(reason);
            Udp.Disconnect(reason);
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
