namespace Inertia.Network
{
    internal sealed class DefaultWebSocketNetworkProtocol : WebSocketNetworkProtocol
    {
        internal DefaultWebSocketNetworkProtocol()
        {
        }

        public override int NetworkBufferLength => 4096;
        public override int MaxReceivedMessagePerSecondPerClient => 60;
    }
}