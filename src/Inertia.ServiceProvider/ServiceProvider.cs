using Inertia.Logging;
using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace Inertia
{
    public static class ServiceProvider
    {
        internal sealed class ServiceProviderTypeInterceptor : ReflectionLoadInterceptor
        {
            internal ServiceProviderTypeInterceptor()
            {
            }

            protected override void Intercept(Type type)
            {
                if (type.GetInterface(nameof(ISingletonService)) != null)
                {
                    _ = _genericAddSingletonMethod
                        .MakeGenericMethod(type)
                        .Invoke(null, null);
                }
                else if (type.GetInterface(nameof(ITransientService)) != null)
                {
                    _ = _genericAddTransientMethod
                        .MakeGenericMethod(type)
                        .Invoke(null, null);
                }
            }
        }

        private readonly static MethodInfo _genericAddSingletonMethod;
        private readonly static MethodInfo _genericAddTransientMethod;
        private static ConcurrentDictionary<Type, IServiceResolver> _singletonServices;
        private static ConcurrentDictionary<Type, IServiceResolver> _transientServices;
        private static ConcurrentDictionary<Type, Type> _serviceInterfacePairs;
        
        static ServiceProvider()
        {
            _genericAddSingletonMethod = typeof(ServiceProvider).GetMethod(nameof(AddSingleton), 1, Type.EmptyTypes);
            _genericAddTransientMethod = typeof(ServiceProvider).GetMethod(nameof(AddTransient), 1, Type.EmptyTypes);
            _singletonServices = new ConcurrentDictionary<Type, IServiceResolver>();
            _transientServices = new ConcurrentDictionary<Type, IServiceResolver>();
            _serviceInterfacePairs = new ConcurrentDictionary<Type, Type>();

            ReflectionProvider.Invalidate();

            AddSingleton<ILogger, DefaultLogger>();
        }

        public static T GetService<T>()
        {
            return (T)GetService(typeof(T));
        }
        public static T GetSingletonService<T>()
        {
            return GetService<T>(_singletonServices);
        }
        public static T GetTransientService<T>()
        {
            return GetService<T>(_transientServices);
        }

        public static void AddSingleton<TInterface, T>()
        {
            AddSingleton<TInterface, T>(null);
        }
        public static void AddSingleton<TInterface, T>(Action<T>? configureService)
        {
            ValidateAndRegisterInterfaceType(typeof(TInterface), typeof(T));
            AddSingleton(configureService);
        }
        public static void AddSingleton<T>()
        {
            AddSingleton<T>(null);
        }
        public static void AddSingleton<T>(Action<T>? configureService)
        {
            var serviceType = typeof(T);

            ValidateServiceType(serviceType);

            _singletonServices[serviceType] = new ServiceResolver<T>(configureService, true);
        }

        public static void AddTransient<TInterface, T>()
        {
            AddTransient<TInterface, T>(null);
        }
        public static void AddTransient<TInterface, T>(Action<T>? configureService)
        {
            ValidateAndRegisterInterfaceType(typeof(TInterface), typeof(T));
            AddTransient(configureService);
        }
        public static void AddTransient<T>()
        {
            AddTransient<T>(null);
        }
        public static void AddTransient<T>(Action<T>? configureService)
        {
            var serviceType = typeof(T);

            ValidateServiceType(serviceType);

            _transientServices[serviceType] = new ServiceResolver<T>(configureService, false);
        }

        internal static object GetService(Type serviceType)
        {
            ValidateServiceType(serviceType);

            serviceType = TryGetPairTypeIfInterfaceOrSelf(serviceType);

            if (_singletonServices.TryGetValue(serviceType, out var resolver)) return resolver.Resolve();
            if (_transientServices.TryGetValue(serviceType, out resolver)) return resolver.Resolve();

            throw new ServiceNotFoundException(serviceType);
        }
        private static T GetService<T>(ConcurrentDictionary<Type, IServiceResolver> services)
        {
            var serviceType = typeof(T);
            ValidateServiceType(serviceType);

            serviceType = TryGetPairTypeIfInterfaceOrSelf(serviceType);
            if (services.TryGetValue(serviceType, out var resolver)) return (T)resolver.Resolve();

            throw new ServiceNotFoundException(typeof(T));
        }
        private static Type TryGetPairTypeIfInterfaceOrSelf(Type type)
        {
            if (type.IsInterface && _serviceInterfacePairs.TryGetValue(type, out var serviceType)) return serviceType;

            return type;
        }
        private static void ValidateServiceType(Type serviceType)
        {
            if (serviceType.IsInterface) return;

            if (!serviceType.IsClass || serviceType.IsValueType)
            {
                throw new ServiceTypeInvalidException(serviceType);
            }
        }
        private static void ValidateAndRegisterInterfaceType(Type interfaceType, Type serviceType)
        {
            if (!interfaceType.IsInterface || !interfaceType.IsAssignableFrom(serviceType))
            {
                throw new InvalidOperationException($"The type '{interfaceType.Name}' must be an interface and the type '{serviceType.Name}' must implement it to be registered as a service.");
            }

            if (_serviceInterfacePairs.ContainsKey(interfaceType))
            {
                throw new ServiceTypeInterfaceAlreadyRegisteredException(interfaceType);
            }

            _serviceInterfacePairs.TryAdd(interfaceType, serviceType);
        }
    }
}
