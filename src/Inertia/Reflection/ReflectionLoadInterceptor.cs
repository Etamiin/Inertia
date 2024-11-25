using System;

namespace Inertia
{
    public abstract class ReflectionLoadInterceptor
    {
        protected ReflectionLoadInterceptor()
        {
        }
        protected ReflectionLoadInterceptor(Predicate<Type>? predicate)
        {
            Predicate = predicate;
        }

        internal Predicate<Type>? Predicate { get; private set; }

        internal protected abstract void Intercept(Type type);
    }
}
