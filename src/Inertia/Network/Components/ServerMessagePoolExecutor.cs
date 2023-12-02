using Inertia.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Inertia.Network
{
    internal class ServerMessagePoolExecutor
    {
        internal int ConnectionPerQueue { get; set; }

        private readonly ILogger _logger;
        private readonly List<ServerMessageQueue> _pool;
        private readonly object _locker;

        internal ServerMessagePoolExecutor(int connectionPerQueue, ILogger logger)
        {
            ConnectionPerQueue = connectionPerQueue;

            _logger = logger;
            _pool = new List<ServerMessageQueue>();
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
                var queue = _pool.FirstOrDefault((q) => q.ConnectionCount < ConnectionPerQueue);
                if (queue == null)
                {
                    queue = new ServerMessageQueue(_logger);
                    _pool.Add(queue);
                }

                return queue;
            }
        }
    }
}