using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Inertia
{
    public sealed class IdProvider
    {
        public static IdProvider CreateNew()
        {
            return new IdProvider(0);
        }
        public static IdProvider CreateFrom(int startId)
        {
            return new IdProvider(startId);
        }

        private int _currentId;

        internal IdProvider(int startId)
        {
            _currentId = startId;
        }

        public int GetId()
        {
            return Interlocked.Increment(ref _currentId);
        }
    }
}
