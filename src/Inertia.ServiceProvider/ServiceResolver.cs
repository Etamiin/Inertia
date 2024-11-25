using System.Linq;
using System.Reflection;
using System;

namespace Inertia
{
    internal class ServiceResolver<T> : IServiceResolver
    {
        private readonly ConstructorInfo _constructor;
        private readonly Type[] _parameterTypes;
        private readonly Action<T> _configureService;
        private readonly bool _isSingleton;
        private T _singletonInstance;

        public ServiceResolver(Action<T> configureService, bool isSingleton)
        {
            var serviceType = typeof(T);
            var constructors = serviceType.GetConstructors();

            if (constructors.Length == 1)
            {
                _constructor = constructors[0];
            }
            else
            {
                _constructor = constructors.FirstOrDefault((ci) => ci.GetCustomAttribute<DefaultServiceConstructor>() != null);
                if (_constructor is null)
                {
                    _constructor = serviceType.GetConstructor(Type.EmptyTypes);
                }
            }

            if (_constructor is null) throw new DefaultServiceConstructorNotFoundException(serviceType);

            _isSingleton = isSingleton;
            _configureService = configureService;
            _parameterTypes = _constructor.GetParameters().Select((p) => p.ParameterType).ToArray();
        }

        public object Resolve()
        {
            if (_singletonInstance != null) return _singletonInstance;

            var parameters = new object[_parameterTypes.Length];
            for (var i = 0; i < _parameterTypes.Length; i++)
            {
                parameters[i] = ServiceProvider.GetService(_parameterTypes[i]);
            }

            var instance = (T)Activator.CreateInstance(typeof(T), parameters);
            _configureService?.Invoke(instance);

            if (_isSingleton)
            {
                _singletonInstance = instance;
            }

            return instance;
        }
    }
}