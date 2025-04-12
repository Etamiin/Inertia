using System;
using System.Collections.Concurrent;

namespace Inertia.IO
{
    public static class DataWriterPool
    {
        private static readonly ConcurrentBag<PooledDataWriter> _pool = new ConcurrentBag<PooledDataWriter>();

        public static PooledDataWriter Rent()
        {
            if (!_pool.TryTake(out var writer))
            {
                writer = new PooledDataWriter();
            }

            writer.IsPooled = true;
            return writer;
        }
        public static void Rent(Action<PooledDataWriter> onWriter)
        {
            using (var writer = Rent())
            {
                onWriter(writer);
            }
        }

        internal static void Return(PooledDataWriter writer)
        {
            if (!writer.IsPooled) return;

            writer.Clear();
            _pool.Add(writer);
        }
    }
}
