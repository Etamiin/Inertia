using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Inertia.Logging;

namespace Inertia
{
    public static class ReflectionProvider
    {            
        private readonly static Dictionary<Type, Dictionary<string, SerializablePropertyCache>> _propertyCaches;
        private readonly static Dictionary<string, Type> _commandExecutionTypePerName;

        static ReflectionProvider()
        {
            _propertyCaches = new Dictionary<Type, Dictionary<string, SerializablePropertyCache>>();
            _commandExecutionTypePerName = new Dictionary<string, Type>();

            RegisterAll();
        }

        public static bool Invalidate() => true;

        internal static bool TryGetSerializableProperties(Type type, out Dictionary<string, SerializablePropertyCache> properties)
        {
            return _propertyCaches.TryGetValue(type, out properties);
        }
        internal static IEnumerable<string> GetCommandNames()
        {
            return _commandExecutionTypePerName.Keys.ToArray();
        }
        internal static CommandExecutor CreateCommandExecutor(string commandName, ILogger logger, object? state)
        {
            if (_commandExecutionTypePerName.TryGetValue(commandName, out var cmdType))
            {
                var cmd = TryCreateInstance<CommandExecutor>(BindingFlags.Public | BindingFlags.Instance, cmdType, Type.EmptyTypes);
                
                cmd.Logger = logger ?? NullLogger.Instance;
                cmd.State = state;

                return cmd;
            }

            return null;
        }

        private static void RegisterAll()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            try
            {
                var penSystemTypes = new List<Type>();
                var bindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;

                foreach (var assembly in assemblies)
                {
                    var types = assembly.GetTypes()
                        .Where((t) => t.IsClass && !t.IsAbstract);

                    var interceptors = types
                        .Where((t) => t.IsSubclassOf(typeof(ReflectionLoadInterceptor)))
                        .Select((t) => TryCreateInstance<ReflectionLoadInterceptor>(bindingFlags, t, Type.EmptyTypes));

                    foreach (var type in types)
                    {
                        ReadTypeIOInformations(type);
                        ReadTypeInformations(type);

                        foreach (var interceptor in interceptors)
                        {
                            interceptor.Intercept(type);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new TypeLoadException($"{nameof(ReflectionProvider)} failed to load.", ex);
            }
        }
        private static T TryCreateInstance<T>(Type owner, Type[] parametersTypes, params object[] parameters)
        {
            return TryCreateInstance<T>(BindingFlags.Instance | BindingFlags.Public, owner, parametersTypes, parameters);
        }
        private static T TryCreateInstance<T>(BindingFlags bindingFlags, Type owner, Type[] parametersTypes, params object[] parameters)
        {
            var constructor = owner.GetConstructor(bindingFlags, Type.DefaultBinder, parametersTypes, new ParameterModifier[0]);
            if (constructor is null) throw new ConstructorNotFoundException(owner, parametersTypes);

            return (T)constructor.Invoke(parameters);
        }
        private static void ReadTypeIOInformations(Type type)
        {
            if (type.GetCustomAttribute<AutoSerializableAttribute>() != null)
            {
                var propertiesDict = new Dictionary<string, SerializablePropertyCache>();
                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where((p) => p.CanRead && p.CanWrite && p.GetCustomAttribute<IgnoreAttribute>() is null);
                
                foreach (var property in properties)
                {
                    propertiesDict.Add(property.Name, new SerializablePropertyCache(property));
                }

                _propertyCaches.Add(type, propertiesDict);
            }
        }
        private static void ReadTypeInformations(Type type)
        {
            if (type.IsSubclassOf(typeof(CommandExecutor)))
            {
                var instance = TryCreateInstance<CommandExecutor>(type, Type.EmptyTypes);
                if (_commandExecutionTypePerName.ContainsKey(instance.Name))
                {
                    throw new DuplicateNameException($"Command '{instance.Name}' already registered.");
                }

                _commandExecutionTypePerName[instance.Name] = type;
            }
        }
    }
}