using System.Reflection;

namespace Inertia.Network
{
    internal class NetworkMessageHandlerMethodPointer
    {
        public NetworkMessageHandlerMethodPointer(object instance, MethodInfo method)
        {
            Instance = instance;
            Method = method;
        }

        public object Instance { get; }
        public MethodInfo Method { get; }
    }
}
