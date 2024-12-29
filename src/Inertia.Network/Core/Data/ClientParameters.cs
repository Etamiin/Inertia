using System;

namespace Inertia.Network
{
    public class ClientParameters : NetworkEntityParameters
    {
        public SafeActionQueue ExecutionQueue { get; private set; }

        public ClientParameters(string ip, int port) : base(ip, port)
        {
            ExecutionQueue = new SafeActionQueueRunner();
        }
        public ClientParameters(string ip, int port, SafeActionQueue executionQueue) : base(ip, port)
        {
            if (executionQueue is null)
            {
                throw new ArgumentNullException(nameof(executionQueue));
            }

            ExecutionQueue = executionQueue;
        }
    }
}