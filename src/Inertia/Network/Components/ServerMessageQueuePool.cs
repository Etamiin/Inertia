using Inertia.Logging;
using System.Collections.Generic;
using System.Linq;

namespace Inertia.Network
{
    internal class ServerMessageQueuePool
    {
        internal int ConnectionPerQueue { get; set; }

        private readonly ILogger _logger;
        private readonly List<ServerMessageQueue> _messageQueuePool;
        private readonly object _locker;

        internal ServerMessageQueuePool(int connectionPerQueue)
        {
            ConnectionPerQueue = connectionPerQueue;

            _logger = LoggingProvider.Logger;
            _messageQueuePool = new List<ServerMessageQueue>();
            _locker = new object();
        }

        internal ServerMessageQueue RegisterConnection(NetworkConnectionEntity connection)
        {
            var queue = GetAvailableQueue();
            queue.RegisterConnection(connection);

            return queue;
        }

        private ServerMessageQueue GetAvailableQueue()
        {
            lock (_locker)
            {
                var queue = _messageQueuePool.FirstOrDefault((q) => q.ConnectionCount < ConnectionPerQueue);
                if (queue == null)
                {
                    queue = new ServerMessageQueue(_logger);
                    _messageQueuePool.Add(queue);
                }

                return queue;
            }
        }
    }
}