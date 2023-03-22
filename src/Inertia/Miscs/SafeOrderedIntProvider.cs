using System.Threading;

namespace Inertia
{
    public sealed class SafeOrderedIntProvider
    {
        private int _currentId;

        public SafeOrderedIntProvider()
        {
        }
        public SafeOrderedIntProvider(int startId)
        {
            _currentId = startId;
        }

        public int NextValue()
        {
            return Interlocked.Increment(ref _currentId);
        }
        public int GetCurrentValue()
        {
            return _currentId;
        }
    }
}