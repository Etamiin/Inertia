using System;
using System.Reflection;

namespace Inertia
{
    internal sealed class ServiceCache<T, TConfig> : IServiceCache where T : InertiaService<TConfig>
    {
        private Type _serviceType;

        internal ServiceCache()
        {
            Configuration = typeof(TConfig).TryInvokeConstructor<TConfig>(BindingFlags.Instance | BindingFlags.Public);
            _serviceType = typeof(T);
        }

        public TConfig Configuration { get; }

        public object GetConfiguration() => Configuration;
        public object Build()
        {
            var service = _serviceType.TryInvokeConstructor<T>(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            service.Configure(Configuration);

            return service;
        }
    }
}
