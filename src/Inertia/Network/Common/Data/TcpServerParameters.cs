namespace Inertia.Network
{
    public class TcpServerParameters : NetworkEntityParameters
    {
        public int BacklogQueueSize { get; set; } = NetworkProtocolFactory.DefaultBacklogSize;

        public TcpServerParameters(int port) : base(port)
        {
        }
        public TcpServerParameters(string ip, int port) : base(ip, port)
        {
        }
    }
}