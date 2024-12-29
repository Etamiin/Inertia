using System.Collections.Generic;
using System.Reflection;

namespace Inertia
{
    public class ReflectionLoaderConfiguration
    {
        public ReflectionLoaderConfiguration()
        {
            Assemblies = new List<Assembly>();
            TypeLoaderInterceptors = new List<TypeLoaderInterceptor>();
        }

        internal List<Assembly> Assemblies { get; }
        internal List<TypeLoaderInterceptor> TypeLoaderInterceptors { get; }

        public ReflectionLoaderConfiguration AddAssemblies(params Assembly[] assemblies)
        {
            Assemblies.AddRange(assemblies);
            
            return this;
        }
        public ReflectionLoaderConfiguration AddInterceptor(TypeLoaderInterceptor interceptor)
        {
            TypeLoaderInterceptors.Add(interceptor);

            return this;
        }
    }
}