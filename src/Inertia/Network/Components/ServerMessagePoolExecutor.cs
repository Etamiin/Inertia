using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inertia.Network
{
    internal class ServerMessagePoolExecutor
    {
        internal int ConnectionPerQueue { get; set; }

        private List<ServerMessageQueue> _pool;
        private object _locker;

        internal ServerMessagePoolExecutor(int connectionPerQueue)
        {
            _pool = new List<ServerMessageQueue>();
            ConnectionPerQueue = connectionPerQueue;
            _locker = new object();
        }

        internal void RegisterConnection(NetworkConnectionEntity connection)
        {
            GetAvailableQueue().RegisterConnection(connection);
        }

        private ServerMessageQueue GetAvailableQueue()
        {
            lock (_locker)
            {
                var queue = _pool.FirstOrDefault((q) => q.ConnectionCount < ConnectionPerQueue);
                if (queue == null)
                {
                    queue = new ServerMessageQueue();
                    _pool.Add(queue);
                }

                return queue;
            }
        }
    }
}