using System;
using System.Linq;
using System.Reflection;

namespace Inertia.Network
{
    public abstract class NetworkEntity
    {
        internal protected Type? WrappedType { get; private set; }

        public NetworkEntity()
        {
            var receiverType = GetType();
            var indirectEntityType = receiverType
                .GetInterfaces()
                .FirstOrDefault((interfaceType) => interfaceType.GetCustomAttribute<IndirectNetworkEntityAttribute>() != null);

            if (indirectEntityType != null)
            {
                WrappedType = indirectEntityType;
            }
        }

        internal abstract void ProcessInQueue(Action action);
    }
}