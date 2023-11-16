using System;

namespace Inertia.Network
{
    public class ClientParameters : NetworkEntityParameters
    {
        public TickedQueue ExecutionQueue { get; private set; }

        public ClientParameters(string ip, int port) : base(ip, port)
        {
            ExecutionQueue = new AsyncTickedQueue();
        }
        public ClientParameters(string ip, int port, TickedQueue executionQueue) : base(ip, port)
        {
            if (executionQueue == null)
            {
                throw new ArgumentNullException(nameof(executionQueue));
            }

            ExecutionQueue = executionQueue;
        }
    }
}