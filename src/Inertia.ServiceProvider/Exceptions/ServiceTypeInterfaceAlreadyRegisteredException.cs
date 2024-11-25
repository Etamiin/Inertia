using System;

namespace Inertia
{
    public class ServiceTypeInterfaceAlreadyRegisteredException : Exception
    {
        public ServiceTypeInterfaceAlreadyRegisteredException(Type interfaceType) : base($"The interface type '{interfaceType.Name}' cannot be used for multiple service registration.")
        {
        }
    }
}