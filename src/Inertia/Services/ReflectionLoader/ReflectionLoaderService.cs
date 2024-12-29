using System.Collections.Generic;
using System;
using Inertia.Logging;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Inertia
{
    public sealed class ReflectionLoaderService : InertiaService<ReflectionLoaderConfiguration>
    {
        private readonly Dictionary<Type, Dictionary<string, SerializablePropertyCache>> _propertyCaches;
        private readonly Dictionary<string, Type> _commandHandlerTypePerName;
        private readonly Type[] _commandHandlerConstructorTypes;

        internal ReflectionLoaderService()
        {
            _propertyCaches = new Dictionary<Type, Dictionary<string, SerializablePropertyCache>>();
            _commandHandlerTypePerName = new Dictionary<string, Type>();
            _commandHandlerConstructorTypes = new Type[] { typeof(ILogger), typeof(object) };
        }

        public override void Configure(ReflectionLoaderConfiguration configuration)
        {
            try
            {
                _propertyCaches.Clear();
                _commandHandlerTypePerName.Clear();

                foreach (var assembly in configuration.Assemblies)
                {
                    var types = assembly.GetTypes()
                        .Where((t) => t.IsClass && !t.IsAbstract);

                    foreach (var type in types)
                    {
                        ReadTypeIOInformations(type);
                        ReadTypeServicesInformations(type);

                        foreach (var interceptor in configuration.TypeLoaderInterceptors)
                        {
                            interceptor.TryIntercept(type);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new TypeLoadException($"{GetType().Name} failed to load.", ex);
            }
        }
        internal bool TryGetSerializableProperties(Type type, out Dictionary<string, SerializablePropertyCache> properties) => _propertyCaches.TryGetValue(type, out properties);
        internal IEnumerable<string> GetAllCommandHandlerNames() => _commandHandlerTypePerName.Keys.ToArray();
        internal CommandHandler CreateCommandHandler(string commandName, ILogger logger, object? state)
        {
            if (_commandHandlerTypePerName.TryGetValue(commandName, out var cmdType))
            {
                logger = logger ?? NullLogger.Instance;

                return cmdType.TryInvokeConstructor<CommandHandler>(_commandHandlerConstructorTypes, logger, state);
            }

            return null;
        }

        private void ReadTypeIOInformations(Type type)
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
        private void ReadTypeServicesInformations(Type type)
        {
            if (type.IsSubclassOf(typeof(CommandHandler)))
            {
                var instance = type.TryInvokeConstructor<CommandHandler>(_commandHandlerConstructorTypes, null, null);
                if (_commandHandlerTypePerName.ContainsKey(instance.CommandName))
                {
                    throw new DuplicateNameException($"Command '{instance.CommandName}' already registered.");
                }

                _commandHandlerTypePerName[instance.CommandName] = type;
            }
        }
    }
}