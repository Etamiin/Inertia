using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Inertia
{
    public sealed class IdProvider
    {
        private int _currentId;

        public IdProvider()
        {
        }
        public IdProvider(int startId)
        {
            _currentId = startId;
        }

        public int NextId()
        {
            return Interlocked.Increment(ref _currentId);
        }
    }
}
