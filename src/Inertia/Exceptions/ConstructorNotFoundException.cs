using System;
using System.Linq;

namespace Inertia
{
    public class ConstructorNotFoundException : Exception
    {
        public override string Message
        {
            get
            {
                if (_parametersType.Length == 0) return $"The object '{_ownerType.Name}' must contain a constructor without parameters.";

                var typesStr = string.Join(", ", _parametersType.Select((type) => type.Name));
                return $"The object '{_ownerType.Name}' must contain a constructor with parameters ({typesStr}).";
            }
        }

        private readonly Type _ownerType;
        private readonly Type[] _parametersType;

        internal ConstructorNotFoundException(Type ownerType, Type[] parametersType)
        {
            _ownerType = ownerType;
            _parametersType = parametersType;
        }
    }
}
