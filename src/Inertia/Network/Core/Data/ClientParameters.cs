using System;

namespace Inertia.Network
{
    public class ClientParameters : NetworkEntityParameters
    {
        public ActionQueue ExecutionQueue { get; private set; }

        public ClientParameters(string ip, int port, ActionQueue executionQueue) : base(ip, port)
        {
            if (executionQueue == null)
            {
                throw new ArgumentNullException(nameof(executionQueue));
            }

            ExecutionQueue = executionQueue;
        }
    }
}