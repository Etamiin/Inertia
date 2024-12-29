using System;

namespace Inertia
{
    public abstract class TypeLoaderInterceptor
    {
        public abstract void TryIntercept(Type type);
    }
}