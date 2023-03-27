using Inertia.Logging;

namespace Inertia.Network
{
    public abstract class ServerParameters
    {
        public ILogger? Logger { get; set; }

        public readonly string Ip;
        public readonly int Port;

        protected ServerParameters(int port) : this(string.Empty, port)
        {
        }
        protected ServerParameters(string ip, int port)
        {
            Ip = ip.Replace("localhost", "127.0.0.1");
            Port = port;
        }
    }
}
