namespace Inertia.Network
{
    public class DefaultWebSocketNetworkProtocol : WebSocketNetworkProtocol
    {
        public override int NetworkBufferLength => 4096;
        public override int ConnectionPerMessageQueue => 500;
    }
}