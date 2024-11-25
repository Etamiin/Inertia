using System;

namespace Inertia
{
    public class ServiceTypeInvalidException : Exception
    {
        public ServiceTypeInvalidException(Type serviceType) : base($"The type '{serviceType.Name}' is not a valid type to be registered as a service.")
        {
        }
    }
}