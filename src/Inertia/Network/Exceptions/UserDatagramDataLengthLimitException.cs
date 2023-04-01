using System;

namespace Inertia.Network
{
    [Obsolete]
    public class UserDatagramDataLengthLimitException : Exception
    {
        public override string Message => $"You can't send more than { ushort.MaxValue } bytes with UDP protocol.";
    }
}