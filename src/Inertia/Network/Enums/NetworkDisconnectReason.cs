namespace Inertia.Network
{
    public enum NetworkDisconnectReason : byte
    {
        Manual = 1,
        ConnectionLost = 2,
        ConnectionFailed = 3,
        ConnectionTimeout = 4,
        InvalidMessageSended = 5,
        InvalidDataReceived = 6,
        SslAuthenticationFailed = 7,
        Spam = 8
    }
}