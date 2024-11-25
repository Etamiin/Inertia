using Inertia.Logging;

namespace Inertia.Network
{
    public class DefaultWebSocketNetworkProtocol : WebSocketNetworkProtocol
    {
        public DefaultWebSocketNetworkProtocol(ILogger logger) : base(logger)
        {
        }

        public override int NetworkBufferLength => 4096;
        public override int ConnectionPerMessageQueue => 500;
    }
}