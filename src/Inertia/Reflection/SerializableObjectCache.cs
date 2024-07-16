using System;
using System.Linq;
using System.Reflection;

namespace Inertia
{
    internal class SerializableObjectCache
    {
        private readonly ConstructorInfo _constructor;
        private bool _isDefaultConstructor;

        internal SerializableObjectCache(Type type)
        {
            _constructor = type.GetConstructor(Type.EmptyTypes);
            _isDefaultConstructor = _constructor != null;

            if (!_isDefaultConstructor)
            {
                _constructor = type.GetConstructors()[0];
            }
        }

        internal object CreateInstance()
        {
            if (_isDefaultConstructor) return _constructor.Invoke(new object[0]);

            var parameters = _constructor.GetParameters()
                .Select(p => null as object)
                .ToArray();

            return _constructor.Invoke(parameters);
        }
    }
}
