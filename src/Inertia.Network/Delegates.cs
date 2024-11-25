namespace Inertia.Network
{
    public delegate void BytesSenderAction(byte[] dataToSend);
    public delegate void WebSocketConnectionEstablished(WebSocketConnectionEntity connection);
}
