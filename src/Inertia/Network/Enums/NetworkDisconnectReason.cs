namespace Inertia.Network
{
    public enum NetworkDisconnectReason : byte
    {
        Manual = 1,
        ConnectionLost = 100,
        ConnectionFailed = 101,
        ConnectionTimeout = 102,
        InvalidMessageSended = 103,
        InvalidDataReceived = 104,
        SslAuthentificationFailed = 105,
        Spam = 106
    }
}
