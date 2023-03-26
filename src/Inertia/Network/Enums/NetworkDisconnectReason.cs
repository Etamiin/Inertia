using System;

namespace Inertia.Network
{
    [Flags]
    public enum NetworkDisconnectReason : byte
    {
        Manual = 1,
        ConnectionLost = 2,
        ConnectionFailed = 4,
        ConnectionTimeout = 8,
        InvalidMessageSended = 16,
        InvalidDataReceived = 32,
        SslAuthentificationFailed = 64,
        Spam = 128
    }
}
