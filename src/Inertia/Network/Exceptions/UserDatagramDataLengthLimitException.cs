using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Network
{
    /// <summary>
    /// Thrown when sending more than <see cref="ushort.MaxValue"/> bytes from UDP protocol
    /// </summary>
    public class UserDatagramDataLengthLimitException : Exception
    {
        public override string Message => $"You can't send more than { ushort.MaxValue } bytes from UDP.";
    }
}