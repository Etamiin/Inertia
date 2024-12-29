using Inertia.Logging;
using System;
using System.Collections.Generic;

namespace Inertia
{
    public sealed class InertiaServices
    {
        private static readonly Lazy<InertiaServices> _lazy = new Lazy<InertiaServices>(() => new InertiaServices());
        private static bool _builded, _defaultServicesAdded;
        
        public static InertiaServices Instance => _lazy.Value;
        public static ILogger Logger { get; private set; } = NullLogger.Instance;

        public static T GetServiceOrThrow<T>()
        {
            var service = GetServiceOrDefault<T>();
            if (service == null)
            {
                throw new KeyNotFoundException($"Service of type '{typeof(T).Name}' not found.");
            }

            return service;
        }
        public static T GetServiceOrDefault<T>()
        {
            if (!_builded)
            {
                throw new InvalidOperationException($"The '{nameof(Build)}' method must be called before accessing services.");
            }

            if (Instance._services.TryGetValue(typeof(T), out var service)) return (T)service;

            return default;
        }

        private readonly Dictionary<Type, object> _services;
        private readonly Dictionary<Type, IServiceCache> _serviceCaches;

        private InertiaServices()
        {
            _services = new Dictionary<Type, object>();
            _serviceCaches = new Dictionary<Type, IServiceCache>();
        }

        public InertiaServices ConfigureDefaultServices()
        {
            if (!_defaultServicesAdded)
            {
                this
                    .AddLoggerModule(new ConsoleLoggerModule())
                    .ConfigureService<ModulableLogger, ModulableLoggerConfiguration>()
                    .ConfigureService<ReflectionLoaderService, ReflectionLoaderConfiguration>((configuration) =>
                    {
                        configuration.AddAssemblies(AppDomain.CurrentDomain.GetAssemblies());
                    });

                _defaultServicesAdded = true;
            }

            return this;
        }
        public InertiaServices ConfigureService<T, TConfig>() where T : InertiaService<TConfig>
        {
            return ConfigureService<T, TConfig>(null);
        }
        public InertiaServices ConfigureService<T, TConfig>(Action<TConfig> onConfiguration) where T : InertiaService<TConfig>
        {
            var serviceType = typeof(T);

            if (!_serviceCaches.TryGetValue(serviceType, out var cache))
            {
                cache = new ServiceCache<T, TConfig>();

                _serviceCaches.Add(serviceType, cache);
            }

            onConfiguration?.Invoke((TConfig)cache.GetConfiguration());

            return this;
        }
        public InertiaServices AddLoggerModule(ILoggerModule module)
        {
            ModulableLogger.DefaultModules.Add(module);

            return this;
        }
        public InertiaServices AddLoggerModules(params ILoggerModule[] modules)
        {
            ModulableLogger.DefaultModules.AddRange(modules);

            return this;
        }
        public InertiaServices UseLogger(ILogger logger)
        {
            Logger = logger;

            return this;
        }

        public void Build()
        {
            if (_builded)
            {
                throw new InvalidOperationException($"The services have already been built.");
            }

            foreach (var cache in _serviceCaches)
            {
                _services[cache.Key] = cache.Value.Build();
            }

            _serviceCaches.Clear();
            _builded = true;

            // Logger is assigned after services are built because there is maybe a ModulableLogger service available to use in case no logger are set.
            if (Logger == null || Logger == NullLogger.Instance)
            {
                Logger = (ILogger)GetServiceOrDefault<ModulableLogger>() ?? NullLogger.Instance;
            }
        }
    }
}
