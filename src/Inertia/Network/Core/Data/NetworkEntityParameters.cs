using Inertia.Logging;

namespace Inertia.Network
{
    public abstract class NetworkEntityParameters
    {
        public ILogger Logger { get; set; }
        public string Ip { get; set; }
        public int Port { get; set; }
        public int MessageCountLimitBeforeSpam { get; set; }

        protected NetworkEntityParameters(int port) : this(string.Empty, port)
        {
        }
        protected NetworkEntityParameters(string ip, int port)
        {
            Logger = LoggingProvider.Logger;
            MessageCountLimitBeforeSpam = NetworkProtocolManager.DefaultMessageCountLimitBeforeSpam;
            Ip = ip.Replace("localhost", "127.0.0.1");
            Port = port;
        }
    }
}