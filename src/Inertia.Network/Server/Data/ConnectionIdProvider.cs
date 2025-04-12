using System.Threading;

namespace Inertia.Network
{
    internal static class ConnectionIdProvider
    {
        private static int _id;

        internal static int GetLastGeneratedId()
        {
            return _id;
        }
        internal static int GetNextId()
        {
            return Interlocked.Increment(ref _id);
        }
        internal static void Reset()
        {
            Interlocked.Exchange(ref _id, 0);
        }
    }
}
