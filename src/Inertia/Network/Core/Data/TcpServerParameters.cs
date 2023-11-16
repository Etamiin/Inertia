namespace Inertia.Network
{
    public class TcpServerParameters : NetworkEntityParameters
    {
        public int BacklogQueueSize { get; set; }

        public TcpServerParameters(int port) : this(string.Empty, port)
        {
        }
        public TcpServerParameters(string ip, int port) : base(ip, port)
        {
            BacklogQueueSize = NetworkProtocolManager.DefaultBacklogSize;
        }
    }
}