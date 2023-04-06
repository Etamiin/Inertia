using System.Threading;

namespace Inertia
{
    public sealed class SafeOrderedIntProvider
    {
        private int _currentValue;

        public SafeOrderedIntProvider()
        {
        }
        public SafeOrderedIntProvider(int startValue)
        {
            _currentValue = startValue;
        }

        public int NextValue()
        {
            return Interlocked.Increment(ref _currentValue);
        }
        public int GetCurrentValue()
        {
            return _currentValue;
        }
    }
}