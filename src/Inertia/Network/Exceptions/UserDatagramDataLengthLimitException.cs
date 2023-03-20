using System;

namespace Inertia.Network
{
    /// <summary>
    /// Thrown when sending more than <see cref="ushort.MaxValue"/> bytes with UDP protocol
    /// </summary>
    public class UserDatagramDataLengthLimitException : Exception
    {
        public override string Message => $"You can't send more than { ushort.MaxValue } bytes with UDP protocol.";
    }
}