using System;

namespace Inertia
{
    public class DefaultServiceConstructorNotFoundException : Exception
    {
        public DefaultServiceConstructorNotFoundException(Type serviceType) : base($"The type '{serviceType.Name}' don't have a default constructor specified to be registered as a service.")
        {
        }
    }
}
