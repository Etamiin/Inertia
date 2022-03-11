using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Inertia
{
    public sealed class SafeIdProvider
    {
        private int _currentId;

        public SafeIdProvider()
        {
        }
        public SafeIdProvider(int startId)
        {
            _currentId = startId;
        }

        public int NextId()
        {
            return Interlocked.Increment(ref _currentId);
        }
    }
}
