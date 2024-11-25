using System;

namespace Inertia.Network
{
    public class ClientParameters : NetworkEntityParameters
    {
        public ActionQueueBase ExecutionQueue { get; private set; }

        public ClientParameters(string ip, int port) : base(ip, port)
        {
            ExecutionQueue = new AutoActionQueue();
        }
        public ClientParameters(string ip, int port, ActionQueueBase executionQueue) : base(ip, port)
        {
            if (executionQueue is null)
            {
                throw new ArgumentNullException(nameof(executionQueue));
            }

            ExecutionQueue = executionQueue;
        }
    }
}