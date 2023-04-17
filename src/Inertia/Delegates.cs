using Inertia.Network;

namespace System
{
    public delegate void BytesSenderAction(byte[] dataToSend);
    public delegate void WebSocketConnectionEstablished(WebSocketConnectionEntity connection);
}