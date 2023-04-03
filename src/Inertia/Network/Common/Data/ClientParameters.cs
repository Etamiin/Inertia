namespace Inertia.Network
{
    public class ClientParameters : NetworkEntityParameters
    {
        public ActionQueuePool ExecutionPool { get; private set; }

        public ClientParameters(string ip, int port, ActionQueuePool executionPool) : base(ip, port)
        {
            ExecutionPool = executionPool;
        }
    }
}
