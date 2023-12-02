using System;
using System.Collections.Generic;
using System.Linq;

namespace Inertia
{
    public static class DependencyResolver
    {
        private static Dictionary<Type, object> _dependencies;

        static DependencyResolver()
        {
            _dependencies = new Dictionary<Type, object>();
        }

        public static void Register<T>(T instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            _dependencies.Add(typeof(T), instance);
        }
        public static void Register<InterfaceType, InstanceType>()
        {
            Register<InterfaceType, InstanceType>(new object[0]);
        }
        public static void Register<InterfaceType, InstanceType>(params object[] constructorArgs)
        {
            var interfaceType = typeof(InterfaceType);
            var instanceType = typeof(InstanceType);

            if (!interfaceType.IsAssignableFrom(instanceType))
            {
                throw new InjectionDependencyException(interfaceType, instanceType);
            }

            var constructorArgsTypes = constructorArgs.Select((o) => o.GetType()).ToArray();
            var constructor = instanceType.GetConstructor(constructorArgsTypes);

            if (constructor == null)
            {
                throw new ConstructorNotFoundException(instanceType, constructorArgsTypes);
            }

            var instance = constructor.Invoke(constructorArgs);
            _dependencies.Add(interfaceType, instance);
        }

        public static InterfaceType Resolve<InterfaceType>()
        {
            if (_dependencies.TryGetValue(typeof(InterfaceType), out var instance))
            {
                return (InterfaceType)instance;
            }

            return default;
        }
    }
}
