namespace Inertia.Network
{
    public enum NetworkStopReason : byte
    {
        Manual = 1,
        ConnectionLost = 2,
        ConnectionFailed = 3,
        ConnectionTimeout = 4,
        InvalidMessageSent = 5,
        InvalidDataReceived = 6,
        SslAuthenticationFailed = 7,
        Spam = 8
    }
}