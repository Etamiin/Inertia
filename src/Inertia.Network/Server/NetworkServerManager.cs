using System;
using System.Linq;

namespace Inertia.Network
{
    internal static class NetworkServerManager
    {
        private static readonly ServerProcessingQueue[] _processingQueues;

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
            return _processingQueues.OrderBy((queue) => queue.ConnectionCount).First();
        }
    }
}
