using Inertia.Logging;
using System;
using System.Collections.Concurrent;

namespace Inertia.Network
{
    public class ClientProcessingQueue
    {
        private readonly ConcurrentQueue<Action> _queue;

        public ClientProcessingQueue()
        {
            _queue = new ConcurrentQueue<Action>();
        }

        public void Enqueue(Action action)
        {
            _queue.Enqueue(action);
        }
        public void Process()
        {
            while (_queue.Count > 0 && _queue.TryDequeue(out var action))
            {
                try
                {
                    action?.Invoke();
                }
                catch (Exception ex)
                {
                    LoggingProvider.LogHandler.Log(LogLevel.Error, "Error processing action in ClientProcessingQueue.", ex);
                }
            }
        }
    }
}
