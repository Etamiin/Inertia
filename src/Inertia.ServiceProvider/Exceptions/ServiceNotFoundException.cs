using System;

namespace Inertia
{
    public class ServiceNotFoundException : Exception
    {
        public ServiceNotFoundException(Type serviceType) : base($"The service type '{serviceType.Name}' is not registered and cannot be resolved.")
        {
        }
    }
}