using System;
using System.Linq;
using System.Threading;

namespace Inertia.Network
{
    internal static class NetworkServerManager
    {
        private static readonly ServerProcessingQueue[] _processingQueues;
        private static int _roundRobinIndex = -1;

        static NetworkServerManager()
        {
            _processingQueues = new ServerProcessingQueue[Environment.ProcessorCount];

            for (int i = 0; i < _processingQueues.Length; i++)
            {
                _processingQueues[i] = new ServerProcessingQueue();
            }
        }

        internal static ServerProcessingQueue GetBestProcessingQueue()
        {
            var selectedIndex = Interlocked.Increment(ref _roundRobinIndex) % _processingQueues.Length;

            return _processingQueues[selectedIndex];
        }
        internal static (int index, int count)[] GetQueueCharges()
        {
            return _processingQueues
                .Select((q, i) => (i, q.ConnectionCount))
                .ToArray();
        }
    }
}
